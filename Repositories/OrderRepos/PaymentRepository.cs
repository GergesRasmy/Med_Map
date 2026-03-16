namespace Med_Map.Repositories.OrderRepos
{
    public class PaymentRepository: IPaymentRepository
    {
        private readonly Mm_Context _context;

        public PaymentRepository(Mm_Context _context)
        {
            this._context = _context;
        }
        public async Task<Payment?> GetByOrderIdAsync(Guid orderId)
        {
            return await _context.Payment
                .Include(p => p.Logs)
                .FirstOrDefaultAsync(p => p.OrderId == orderId);
        }

        public async Task<Payment?> GetByProviderOrderIdAsync(string providerOrderId)
        {
            return await _context.Payment
                .Include(p => p.Logs)
                .FirstOrDefaultAsync(p => p.ProviderOrderId == providerOrderId);
        }

        public async Task AddAsync(Payment payment)
        {
            await _context.Payment.AddAsync(payment);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }


    }
}
