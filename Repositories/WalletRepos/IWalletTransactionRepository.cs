namespace Med_Map.Repositories.WalletRepos
{
    public interface IWalletTransactionRepository
    {
        Task<WalletTransaction> DepositAsync(Guid walletId, decimal amount, CurrencyType currency, Guid? orderId = null);
        Task<WalletTransaction?> GetByIdAsync(Guid id, bool asNoTracking = true);
        Task<(List<WalletTransaction> items, int totalCount)> GetByWalletIdAsync(Guid walletId, int page, int pageSize = 20);
        Task<(List<WalletTransaction> items, int totalCount)> GetPendingWithdrawalsAsync(int page, int pageSize = 20);
        Task<bool> HasPendingWithdrawalAsync(Guid walletId);
        Task InsertAsync(WalletTransaction transaction);
        Task SaveChangesAsync();
    }
}
