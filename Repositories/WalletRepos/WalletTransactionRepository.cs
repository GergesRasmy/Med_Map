namespace Med_Map.Repositories.WalletRepos
{
    public class WalletTransactionRepository : IWalletTransactionRepository
    {
        private readonly Mm_Context _context;

        public WalletTransactionRepository(Mm_Context context)
        {
            _context = context;
        }

        public async Task<WalletTransaction> DepositAsync(Guid walletId, decimal amount, CurrencyType currency, Guid? orderId = null)
        {
            var wallet = await _context.Wallet.FindAsync(walletId)
                ?? throw new InvalidOperationException($"Wallet {walletId} not found.");

            var transaction = new WalletTransaction
            {
                Type = TransactionType.Deposit,
                Status = TransactionStatus.Completed,
                Amount = amount,
                Currency = currency,
                OrderId = orderId,
                WalletId = walletId,
            };

            wallet.CurrentBalance += amount;
            wallet.TotalEarnings += amount;

            await _context.WalletTransaction.AddAsync(transaction);
            await _context.SaveChangesAsync();

            return transaction;
        }

        public async Task<WalletTransaction?> GetByIdAsync(Guid id, bool asNoTracking = true)
        {
            var query = _context.WalletTransaction.Where(t => t.Id == id);
            if (asNoTracking)
                query = query.AsNoTracking();
            return await query.FirstOrDefaultAsync();
        }

        public async Task<(List<WalletTransaction> items, int totalCount)> GetByWalletIdAsync(Guid walletId, int page, int pageSize = 20)
        {
            var query = _context.WalletTransaction
                .AsNoTracking()
                .Where(t => t.WalletId == walletId);

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderByDescending(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<(List<WalletTransaction> items, int totalCount)> GetPendingWithdrawalsAsync(int page, int pageSize = 20)
        {
            var query = _context.WalletTransaction
                .AsNoTracking()
                .Include(t => t.Wallet)
                    .ThenInclude(w => w.Pharmacy)
                        .ThenInclude(p => p.ActiveProfile)
                .Where(t => t.Type == TransactionType.Withdrawal && t.Status == TransactionStatus.Pending);

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(t => t.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<bool> HasPendingWithdrawalAsync(Guid walletId)
        {
            return await _context.WalletTransaction
                .AnyAsync(t => t.WalletId == walletId
                            && t.Type == TransactionType.Withdrawal
                            && t.Status == TransactionStatus.Pending);
        }

        public async Task InsertAsync(WalletTransaction transaction)
        {
            await _context.WalletTransaction.AddAsync(transaction);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
