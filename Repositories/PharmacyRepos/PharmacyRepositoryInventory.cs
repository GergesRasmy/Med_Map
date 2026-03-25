using Med_Map.Models.pharmacy;
using NetTopologySuite;

namespace Med_Map.Repositories.PharmacyRepos
{
    public class PharmacyInventoryRepository : IPharmacyInventoryRepository
    {
        private readonly Mm_Context _context;

        public PharmacyInventoryRepository(Mm_Context context)
        {
            _context = context;
        }

     
        public async Task<PharmacyInventory?> GetPharmacyMedicineAsync(string pharmacyId, Guid medicineId)
        {
            return await _context.PharmacyInventory
                .FirstOrDefaultAsync(pm => pm.PharmacyProfileId == Guid.Parse(pharmacyId) && pm.MedicineId == medicineId);
        }
        public async Task AddMedicineAsync(PharmacyInventory medicine)
        {
            await _context.PharmacyInventory.AddAsync(medicine);
            await _context.SaveChangesAsync();
        }
        public async Task<bool> RemoveMedicineAsync(string pharmacyId, Guid medicineId)
        {
            // Find the exact record
            var inventoryItem = await _context.PharmacyInventory
                .FirstOrDefaultAsync(pi => pi.PharmacyProfileId == Guid.Parse(pharmacyId) && pi.MedicineId == medicineId);

            if (inventoryItem == null)
                return false;

            // Remove and save
            _context.PharmacyInventory.Remove(inventoryItem);
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<(List<PharmacyInventory> items, int totalCount)> GetPharmacyInventoryAsync(string pharmacyProfileId, int page, int pageSize = 10)
        {
            var query = _context.PharmacyInventory
                .AsNoTracking()
                .Include(pi => pi.Medicine)
                .Where(pi => pi.PharmacyProfileId == Guid.Parse(pharmacyProfileId));

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
