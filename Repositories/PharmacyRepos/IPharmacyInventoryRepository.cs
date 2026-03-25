namespace Med_Map.Repositories.PharmacyRepos
{
    public interface IPharmacyInventoryRepository
    {
        Task AddMedicineAsync(PharmacyInventory medicine);
        Task<PharmacyInventory?> GetPharmacyMedicineAsync(string pharmacyId, Guid medicineId);
        Task<bool> RemoveMedicineAsync(string pharmacyId, Guid medicineId);
        Task SaveChangesAsync();
        Task<(List<PharmacyInventory> items, int totalCount)> GetPharmacyInventoryAsync(string pharmacyProfileId, int page, int pageSize = 10);
        
    }
}
