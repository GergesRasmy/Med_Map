namespace Med_Map.Repositories.Account
{
    public class PharmacyRepository: IPharmacyRepository
    {
        private readonly Mm_Context _context;

        public PharmacyRepository(Mm_Context context)
        {
            _context = context;
        }

        public async Task InsertAsync(Pharmacy pharmacy)
        {
            await _context.Pharmacy.AddAsync(pharmacy);
            await _context.SaveChangesAsync();
        }

        public async Task<Pharmacy> GetByIdAsync(Guid id)
        {
            return await _context.Pharmacy.FindAsync(id);
        }

        public async Task UpdateAsync(Pharmacy pharmacy)
        {
            _context.Pharmacy.Update(pharmacy);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
