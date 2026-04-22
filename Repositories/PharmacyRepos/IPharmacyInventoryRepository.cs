namespace Med_Map.Repositories.PharmacyRepos
{
    public interface IPharmacyInventoryRepository
    {
        Task AddMedicineAsync(PharmacyInventory medicine);
        Task<PharmacyInventory?> GetPharmacyMedicineAsync(string pharmacyId, Guid medicineId);
        Task<bool> RemoveMedicineAsync(string pharmacyId, Guid medicineId);
        Task SaveChangesAsync();
        Task<(List<PharmacyInventory> items, int totalCount)> GetPharmacyInventoryAsync(string pharmacyProfileId, int page, int pageSize = 10);
        Task<PharmacyInventory?> GetPharmacyMedicineBatchAsync(string pharmacyProfileId, string medicineId, DateOnly expiryDate);
        Task<List<PharmacyInventory>> GetMedicineBatchesAsync(string pharmacyProfileId, string medicineId);

    }
}
