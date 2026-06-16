namespace Med_Map.Repositories.OrderRepos
{
    public interface IPaymentRepository
    {
        Task<Payment?> GetByIdAsync(Guid paymentId);
        Task<Payment?> GetByOrderIdAsync(Guid orderId);
        Task<Payment?> GetByProviderOrderIdAsync(string providerOrderId);
        Task AddAsync(Payment payment);
        Task AddLogAsync(PaymentLog log);
        Task SaveChangesAsync();
    }
}
