using Med_Map.Models.pharmacy;

namespace Med_Map.Repositories
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
            _context.Pharmacy.Add(pharmacy);
            await _context.SaveChangesAsync();
        }

        public async Task<Pharmacy?> GetByIdAsync(string id)
        {
            return await _context.Pharmacy
                        .Include(p => p.Documents)   
                        .Include(p => p.PhoneNumbers) 
                        .FirstOrDefaultAsync(p => p.ApplicationUserId == id);
            
        }
        public async Task<List<Pharmacy>> GetByNameAsync(string name)
        {
            string normalizedSearch = name.ToUpper();

            return await _context.Pharmacy
             .Include(p => p.PhoneNumbers)
             .Include(p => p.User) 
             .Where(p =>
                 p.doctorName.ToUpper().Contains(normalizedSearch) ||
                 p.User.NormalizedUserName.Contains(normalizedSearch)) 
             .ToListAsync();
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
