namespace Med_Map.Services
{
    public interface IPaymobService
    {
        Task<string> CreateIntentionAsync(decimal amount, string paymentId);
        bool VerifySignature(string payload, string receivedHmac);
    }
}
