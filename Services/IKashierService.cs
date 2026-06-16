using System.Text.Json;

namespace Med_Map.Services
{
    public interface IKashierService
    {
        /// <summary>
        /// Builds the Kashier Hosted Payment Page URL the client opens (in a WebView on Flutter).
        /// No network call is made — the URL is signed locally with an HMAC-SHA256 hash.
        /// </summary>
        /// <param name="amount">Order total.</param>
        /// <param name="orderId">Our own reference (we pass the internal Payment.Id).</param>
        string BuildPaymentUrl(decimal amount, string orderId);

        /// <summary>
        /// Verifies the Kashier webhook signature. Kashier puts <c>hash</c> at the root of the
        /// payload and <c>signatureKeys</c> inside the nested <c>data</c> object.
        /// </summary>
        bool VerifyWebhookSignature(JsonElement root, string? headerSignature = null);
    }
}
