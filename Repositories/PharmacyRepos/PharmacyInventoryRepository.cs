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

        public async Task<PharmacyInventory?> GetPharmacyMedicineBatchAsync(string pharmacyUserId, string medicineId, DateOnly expiryDate)
        {
            return await _context.PharmacyInventory
                .FirstOrDefaultAsync(pi =>
                    pi.PharmacyUserId == pharmacyUserId &&
                    pi.MedicineId.ToString() == medicineId &&
                    pi.ExpiryDate == expiryDate);
        }

        public async Task<List<PharmacyInventory>> GetMedicineBatchesAsync(string pharmacyUserId, string medicineId)
        {
            return await _context.PharmacyInventory
                .Include(pi => pi.Medicine)
                .Where(pi =>
                    pi.PharmacyUserId == pharmacyUserId &&
                    pi.MedicineId.ToString() == medicineId)
                .OrderBy(pi => pi.ExpiryDate)
                .ToListAsync();
        }

        public async Task<PharmacyInventory?> GetBatchByIdAsync(string pharmacyUserId, Guid batchId)
        {
            return await _context.PharmacyInventory
                .FirstOrDefaultAsync(pi =>
                    pi.PharmacyUserId == pharmacyUserId &&
                    pi.Id == batchId);
        }

        public async Task<bool> RemoveBatchByIdAsync(string pharmacyUserId, Guid batchId)
        {
            var batch = await _context.PharmacyInventory
                .FirstOrDefaultAsync(pi =>
                    pi.PharmacyUserId == pharmacyUserId &&
                    pi.Id == batchId);

            if (batch == null) return false;

            _context.PharmacyInventory.Remove(batch);
            await _context.SaveChangesAsync();
            return true;
        }
        public async Task<PharmacyInventory?> GetPharmacyMedicineAsync(string pharmacyUserId, Guid medicineId)
        {
            return await _context.PharmacyInventory
                .FirstOrDefaultAsync(pm => pm.PharmacyUserId == pharmacyUserId && pm.MedicineId == medicineId);
        }
        public async Task AddMedicineAsync(PharmacyInventory medicine)
        {
            await _context.PharmacyInventory.AddAsync(medicine);
            await _context.SaveChangesAsync();
        }
        public async Task<bool> RemoveMedicineAsync(string pharmacyUserId, Guid medicineId)
        {
            var inventoryItem = await _context.PharmacyInventory
                .FirstOrDefaultAsync(pi => pi.PharmacyUserId == pharmacyUserId && pi.MedicineId == medicineId);

            if (inventoryItem == null)
                return false;

            _context.PharmacyInventory.Remove(inventoryItem);
            await _context.SaveChangesAsync();

            return true;
        }
        public async Task<(List<PharmacyInventory> items, int totalCount)> GetPharmacyInventoryAsync(string pharmacyUserId, int page, int pageSize = 10)
        {
            var query = _context.PharmacyInventory
                .AsNoTracking()
                .Include(pi => pi.Medicine)
                .Where(pi => pi.PharmacyUserId == pharmacyUserId);

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
