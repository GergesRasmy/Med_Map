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
        /// Verifies the signature on a Kashier server webhook payload's <c>data</c> object.
        /// Kashier sends a <c>signatureKeys</c> array telling us which fields to sign, in order.
        /// </summary>
        bool VerifyWebhookSignature(JsonElement data);
    }
}
