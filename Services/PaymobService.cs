using System.Text.Json;

namespace Med_Map.Services
{
    public class PaymobService : IPaymobService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<PaymobService> _logger;

        public PaymobService(HttpClient httpClient, IConfiguration config, ILogger<PaymobService> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;
        }

        public async Task<string> CreateIntentionAsync(decimal amount, string paymentId)
        {
            var response = await _httpClient.PostAsJsonAsync("https://accept.paymob.com/v1/intention/", new
            {
                amount = (int)(amount * 100),
                currency = "EGP",
                payment_methods = new[] { int.Parse(_config["Paymob:IntegrationId"]!) },
                items = Array.Empty<object>(),
                billing_data = new
                {
                    first_name = "NA", last_name = "NA",
                    email = "NA", phone_number = "NA"
                },
                special_reference = paymentId
            });

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogError("Paymob intention creation failed {Status}: {Body}", response.StatusCode, body);
                throw new Exception("Failed to create Paymob payment intention");
            }

            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            return result.GetProperty("client_secret").GetString()
                ?? throw new Exception("Paymob client_secret was null");
        }

        public bool VerifySignature(string payload, string receivedHmac)
        {
            var secret = _config["Paymob:HmacSecret"]!;

            JsonElement obj;
            try
            {
                var root = JsonSerializer.Deserialize<JsonElement>(payload);
                obj = root.GetProperty("obj");
            }
            catch
            {
                _logger.LogWarning("HMAC verification failed: could not parse webhook payload");
                return false;
            }

            // Paymob HMAC: 20 fields from obj in lexicographic order, values concatenated, then HMAC-SHA512
            var concatenated = string.Concat(new[]
            {
                GetField(obj, "amount_cents"),
                GetField(obj, "created_at"),
                GetField(obj, "currency"),
                GetField(obj, "error_occured"),         // Paymob typo — one 'r'
                GetField(obj, "has_parent_transaction"),
                GetField(obj, "id"),
                GetField(obj, "integration_id"),
                GetField(obj, "is_3d_secure"),
                GetField(obj, "is_auth"),
                GetField(obj, "is_capture"),
                GetField(obj, "is_refunded"),
                GetField(obj, "is_standalone_payment"),
                GetField(obj, "is_voided"),
                GetNestedField(obj, "order", "id"),
                GetField(obj, "owner"),
                GetField(obj, "pending"),
                GetNestedField(obj, "source_data", "pan"),
                GetNestedField(obj, "source_data", "sub_type"),
                GetNestedField(obj, "source_data", "type"),
                GetField(obj, "success"),
            });

            var computedHmac = ComputeHmac(concatenated, secret);
            var match = computedHmac.Equals(receivedHmac, StringComparison.OrdinalIgnoreCase);

            if (!match)
                _logger.LogWarning("HMAC mismatch. Computed {Computed}, received {Received}", computedHmac, receivedHmac);

            return match;
        }

        private static string GetField(JsonElement element, string key)
        {
            if (!element.TryGetProperty(key, out var prop)) return "";
            return prop.ValueKind switch
            {
                JsonValueKind.String => prop.GetString() ?? "",
                JsonValueKind.True   => "true",
                JsonValueKind.False  => "false",
                JsonValueKind.Null   => "",
                _                    => prop.ToString()
            };
        }

        private static string GetNestedField(JsonElement element, string parent, string key)
        {
            if (!element.TryGetProperty(parent, out var parentProp)) return "";
            return GetField(parentProp, key);
        }

        private static string ComputeHmac(string data, string secret)
        {
            using var hmac = new System.Security.Cryptography.HMACSHA512(
                System.Text.Encoding.UTF8.GetBytes(secret));
            var hash = hmac.ComputeHash(System.Text.Encoding.UTF8.GetBytes(data));
            return Convert.ToHexString(hash).ToLower();
        }
    }
}
