namespace Med_Map.Repositories
{
    public interface IPharmacyRepository
    {
        Task InsertAsync(Pharmacy pharmacy);
        Task<Pharmacy> GetByIdAsync(Guid id);
        Task UpdateAsync(Pharmacy pharmacy);
        Task SaveChangesAsync();
    }
}
