namespace Med_Map.Repositories.WalletRepos
{
    public class WalletRepository : IWalletRepository
    {
        private readonly Mm_Context _context;

        public WalletRepository(Mm_Context context)
        {
            _context = context;
        }

        public async Task InsertAsync(Wallet wallet)
        {
            await _context.Wallet.AddAsync(wallet);
            await SaveChangesAsync();
        }

        public async Task<Wallet?> GetByIdAsync(Guid walletId)
        {
            return await _context.Wallet.FindAsync(walletId);
        }

        public async Task<Wallet?> GetByPharmacyProfileIdAsync(Guid profileId, bool asNoTracking = false)
        {
            var query = _context.Wallet.Where(w => w.PharmacyProfileId == profileId);
            if (asNoTracking)
                query = query.AsNoTracking();
            return await query.FirstOrDefaultAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
