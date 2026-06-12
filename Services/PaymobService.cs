using System.Text.Json;

namespace Med_Map.Services
{
    public class PaymobService : IPaymobService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<PaymobService> _logger;

        private const string BaseUrl = "https://accept.paymob.com/api";

        public PaymobService(HttpClient httpClient, IConfiguration config, ILogger<PaymobService> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;
        }

        public async Task<(string paymentUrl, string providerOrderId)> CreatePaymentUrlAsync(decimal amount, string orderId)
        {
            // Step 1 — Get auth token
            var authToken = await GetAuthTokenAsync();

            // Step 2 — Register order with Paymob
            var providerOrderId = await RegisterOrderAsync(authToken, amount, orderId);

            // Step 3 — Get payment key and build URL
            var paymentKey = await GetPaymentKeyAsync(authToken, providerOrderId, amount);

            var iframeId = _config["Paymob:IframeId"];
            var paymentUrl = $"https://accept.paymob.com/api/acceptance/iframes/{iframeId}?payment_token={paymentKey}";

            return (paymentUrl, providerOrderId);
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

            // Paymob HMAC: extract these 20 fields from obj, already in lexicographic order,
            // concatenate their string values, then HMAC-SHA512
            var concatenated = string.Concat(new[]
            {
                GetField(obj, "amount_cents"),
                GetField(obj, "created_at"),
                GetField(obj, "currency"),
                GetField(obj, "error_occured"),        // Paymob typo — one 'r'
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
                _logger.LogWarning("HMAC mismatch. Computed {Expected}, received {Received}", computedHmac, receivedHmac);

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

        private async Task<string> GetAuthTokenAsync()
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/auth/tokens", new
            {
                api_key = _config["Paymob:ApiKey"]
            });

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Paymob auth failed with status {Status}", response.StatusCode);
                throw new Exception("Failed to authenticate with Paymob");
            }

            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            return result.GetProperty("token").GetString()
                ?? throw new Exception("Paymob auth token was null");
        }

        private async Task<string> RegisterOrderAsync(string authToken, decimal amount, string orderId)
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/ecommerce/orders", new
            {
                auth_token = authToken,
                delivery_needed = false,
                amount_cents = (int)(amount * 100),
                currency = "EGP",
                merchant_order_id = orderId
            });

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Paymob order registration failed with status {Status}", response.StatusCode);
                throw new Exception("Failed to register order with Paymob");
            }

            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            return result.GetProperty("id").GetInt32().ToString()
                ?? throw new Exception("Paymob order ID was null");
        }

        private async Task<string> GetPaymentKeyAsync(string authToken, string providerOrderId, decimal amount)
        {
            var response = await _httpClient.PostAsJsonAsync($"{BaseUrl}/acceptance/payment_keys", new
            {
                auth_token = authToken,
                amount_cents = (int)(amount * 100),
                expiration = 3600,
                order_id = providerOrderId,
                billing_data = new
                {
                    email = "test@test.com",
                    first_name = "Test",
                    last_name = "User",
                    phone_number = "01000000000",
                    apartment = "NA",
                    floor = "NA",
                    street = "NA",
                    building = "NA",
                    shipping_method = "NA",
                    postal_code = "NA",
                    city = "NA",
                    country = "EG",
                    state = "NA"
                },
                currency = "EGP",
                integration_id = int.Parse(_config["Paymob:IntegrationId"]!)
            });

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Paymob payment key request failed with status {Status}", response.StatusCode);
                throw new Exception("Failed to get payment key from Paymob");
            }

            var result = await response.Content.ReadFromJsonAsync<JsonElement>();
            return result.GetProperty("token").GetString()
                ?? throw new Exception("Paymob payment key was null");
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