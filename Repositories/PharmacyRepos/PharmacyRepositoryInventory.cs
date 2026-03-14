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
                .FirstOrDefaultAsync(pm => pm.PharmacyId == pharmacyId && pm.MedicineId == medicineId);
        }
        public async Task AddMedicineAsync(PharmacyInventory medicine)
        {
            await _context.PharmacyInventory.AddAsync(medicine);
            await _context.SaveChangesAsync();
        }
        public async Task<bool> RemoveMedicineAsync(string pharmacyId, Guid medicineId)
        {
            // 1. Find the exact record
            var inventoryItem = await _context.PharmacyInventory
                .FirstOrDefaultAsync(pi => pi.PharmacyId == pharmacyId && pi.MedicineId == medicineId);

            if (inventoryItem == null)
                return false;

            // 2. Remove and save
            _context.PharmacyInventory.Remove(inventoryItem);
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
