namespace Med_Map.Repositories.PharmacyRepos
{
    public interface IPharmacyRepository
    {
        Task SaveToPendingAsync(string userId, PharmacyProfile profile);
        Task UpdateInstantFieldsAsync(string userId, PharmacyUpdateDTO fields);
        Task<bool> ActivateProfileAsync(string userId);
        Task<Pharmacy?> GetByIdAsync(string id);
        Task<List<Pharmacy>> GetByNameAsync(string name);
        Task<List<Pharmacy>> GetNearestPharmacyAsync(double latitude, double longitude, double radiusInMeters);
        Task SaveChangesAsync();
    }
}
