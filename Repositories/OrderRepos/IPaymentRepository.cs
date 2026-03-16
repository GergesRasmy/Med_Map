namespace Med_Map.Repositories.OrderRepos
{
    public interface IPaymentRepository
    {
        Task<Payment?> GetByOrderIdAsync(Guid orderId);
        Task<Payment?> GetByProviderOrderIdAsync(string providerOrderId);
        Task AddAsync(Payment payment);
        Task SaveChangesAsync();
    }
}
