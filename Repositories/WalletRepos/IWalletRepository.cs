namespace Med_Map.Repositories.WalletRepos
{
    public interface IWalletRepository
    {
        Task InsertAsync(Wallet wallet);
        Task<Wallet?> GetByPharmacyProfileIdAsync(Guid profileId, bool asNoTracking = false);
        Task SaveChangesAsync();
    }
}
