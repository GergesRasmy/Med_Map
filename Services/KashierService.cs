using System.Globalization;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Med_Map.Services
{
    public class KashierService : IKashierService
    {
        private readonly IConfiguration _config;
        private readonly ILogger<KashierService> _logger;

        public KashierService(IConfiguration config, ILogger<KashierService> logger)
        {
            _config = config;
            _logger = logger;
        }

        public string BuildPaymentUrl(decimal amount, string orderId)
        {
            var mid = _config["Kashier:MerchantId"]!;
            var secret = _config["Kashier:PaymentApiKey"]!;
            const string currency = "EGP";

            // Invariant culture so the decimal point is "." regardless of server locale.
            var amountStr = amount.ToString("0.00", CultureInfo.InvariantCulture);

            // Kashier order hash: HMAC-SHA256 over "/?payment={mid}.{orderId}.{amount}.{currency}"
            var path = $"/?payment={mid}.{orderId}.{amountStr}.{currency}";
            var hash = ComputeHmacHex(path, secret);

            var mode = _config["Kashier:Mode"] ?? "test";          // "test" or "live"
            var redirect = Uri.EscapeDataString(_config["Kashier:MerchantRedirect"] ?? "");
            var webhook = _config["Kashier:ServerWebhook"];

            var url =
                $"https://checkout.kashier.io/?merchantId={Uri.EscapeDataString(mid)}" +
                $"&orderId={Uri.EscapeDataString(orderId)}" +
                $"&amount={amountStr}" +
                $"&currency={currency}" +
                $"&hash={hash}" +
                $"&mode={mode}" +
                $"&merchantRedirect={redirect}" +
                $"&allowedMethods=card,wallet" +
                $"&display=en";

            // Optional: point Kashier at our server-to-server webhook. Can also be set in the dashboard.
            if (!string.IsNullOrWhiteSpace(webhook))
                url += $"&serverWebhook={Uri.EscapeDataString(webhook)}";

            return url;
        }

        public bool VerifyWebhookSignature(JsonElement data)
        {
            var secret = _config["Kashier:PaymentApiKey"]!;

            try
            {
                if (!data.TryGetProperty("signatureKeys", out var keysEl) ||
                    keysEl.ValueKind != JsonValueKind.Array ||
                    !data.TryGetProperty("signature", out var sigEl))
                {
                    _logger.LogWarning("Kashier webhook missing signatureKeys or signature");
                    return false;
                }

                // Rebuild the query string Kashier signed: "key=value&key=value..." in signatureKeys order.
                var parts = new List<string>();
                foreach (var keyEl in keysEl.EnumerateArray())
                {
                    var key = keyEl.GetString();
                    if (string.IsNullOrEmpty(key)) continue;
                    parts.Add($"{key}={GetField(data, key)}");
                }

                var queryString = string.Join("&", parts);
                var computed = ComputeHmacHex(queryString, secret);
                var received = sigEl.GetString() ?? "";

                var match = computed.Equals(received, StringComparison.OrdinalIgnoreCase);
                if (!match)
                    _logger.LogWarning("Kashier signature mismatch. Computed {Computed} from \"{Qs}\", received {Received}",
                        computed, queryString, received);

                return match;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Kashier signature verification threw");
                return false;
            }
        }

        private static string GetField(JsonElement element, string key)
        {
            if (!element.TryGetProperty(key, out var prop)) return "";
            return prop.ValueKind switch
            {
                JsonValueKind.String => prop.GetString() ?? "",
                JsonValueKind.True => "true",
                JsonValueKind.False => "false",
                JsonValueKind.Null => "",
                _ => prop.ToString()
            };
        }

        private static string ComputeHmacHex(string data, string secret)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(data));
            return Convert.ToHexString(hash).ToLower();
        }
    }
}
