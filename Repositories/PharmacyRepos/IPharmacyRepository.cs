namespace Med_Map.Repositories.PharmacyRepos
{
    public interface IPharmacyRepository
    {
        Task InsertAsync(Pharmacy pharmacy);
        Task<Pharmacy?> GetByIdAsync(string id);
        Task<List<Pharmacy>> GetByNameAsync(string name);
        Task<List<Pharmacy>> GetNearestPharmacyAsync(double latitude, double longitude, double radiusInMetersS);
        Task SaveChangesAsync();
    }
}
