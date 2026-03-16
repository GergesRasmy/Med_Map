namespace Med_Map.Services
{
    public interface IPaymobService
    {
        Task<(string paymentUrl, string providerOrderId)> CreatePaymentUrlAsync(decimal amount, string orderId);
        bool VerifySignature(string payload, string receivedHmac);
    }
}
