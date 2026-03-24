namespace Med_Map.Repositories.PharmacyRepos
{
    public interface IPharmacyRepository
    {
        Task SaveToPendingAsync(string userId, PharmacyProfile profile);
        Task UpdateInstantFieldsAsync(string userId, PharmacyUpdateDTO fields);
        Task<bool> ActivateProfileAsync(string userId);
        Task<Pharmacy?> GetByIdAsync(string id);
        Task<(List<Pharmacy>? items, int totalCount)> GetByNameAsync(string name, int page, int pageSize = 10);
        Task<(List<Pharmacy> items, int totalCount)> GetNearestPharmacyAsync(
                double latitude,
                double longitude,
                double radiusInMeters,
                int page,
                int pageSize = 10);
            Task SaveChangesAsync();
        Task InsertAsync(Pharmacy pharmacy);
        Task<Pharmacy?> GetByIdWithPendingAsync(string id);
    }
}
