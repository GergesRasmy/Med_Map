using Med_Map.Models.pharmacy;

namespace Med_Map.Repositories.Account
{
    public interface IPharmacyRepository
    {
        Task InsertAsync(Pharmacy pharmacy);
        Task<Pharmacy> GetByIdAsync(Guid id);
        Task UpdateAsync(Pharmacy pharmacy);
        Task SaveChangesAsync();
    }
}
