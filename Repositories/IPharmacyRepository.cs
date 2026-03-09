namespace Med_Map.Repositories
{
    public interface IPharmacyRepository
    {
        Task InsertAsync(Pharmacy pharmacy);
        Task<Pharmacy?> GetByIdAsync(string id);
        Task<List<Pharmacy>> GetByNameAsync(string name);

        Task UpdateAsync(Pharmacy pharmacy);
        Task SaveChangesAsync();
    }
}
