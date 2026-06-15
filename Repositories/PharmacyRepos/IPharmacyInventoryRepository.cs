namespace Med_Map.Repositories.PharmacyRepos
{
    public interface IPharmacyInventoryRepository
    {
        Task AddMedicineAsync(PharmacyInventory medicine);
        Task<PharmacyInventory?> GetPharmacyMedicineAsync(string pharmacyId, Guid medicineId);
        Task<PharmacyInventory?> GetPharmacyMedicineWithDetailsAsync(string pharmacyId, Guid medicineId);
        Task<bool> RemoveMedicineAsync(string pharmacyId, Guid medicineId);
        Task SaveChangesAsync();
        Task<(List<PharmacyInventory> items, int totalCount)> GetPharmacyInventoryAsync(string pharmacyUserId, int page, int pageSize = 10);
        Task<PharmacyInventory?> GetPharmacyMedicineBatchAsync(string pharmacyUserId, string medicineId, DateOnly expiryDate);
        Task<List<PharmacyInventory>> GetMedicineBatchesAsync(string pharmacyUserId, string medicineId);
        Task<PharmacyInventory?> GetBatchByIdAsync(string pharmacyUserId, Guid batchId);
        Task<bool> RemoveBatchByIdAsync(string pharmacyUserId, Guid batchId);
        Task<(List<PharmacyInventory> items, int totalCount)> SearchInventoryAsync(string term, string? pharmacyUserId, int page, int pageSize);

    }
}
