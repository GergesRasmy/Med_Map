namespace Med_Map.Repositories.WalletRepos
{
    public interface IWalletRepository
    {
        Task InsertAsync(Wallet wallet);
        Task<Wallet?> GetByIdAsync(Guid walletId);
        Task<Wallet?> GetByPharmacyUserIdAsync(string pharmacyUserId, bool asNoTracking = false);
        Task SaveChangesAsync();
    }
}
