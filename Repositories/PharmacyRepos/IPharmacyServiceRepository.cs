using Med_Map.DTO.PharmacyServiceDTOs;

namespace Med_Map.Repositories.PharmacyRepos
{
    public interface IPharmacyServiceRepository
    {
        Task AddAsync(PharmacyService service);
        Task<PharmacyService?> GetByIdAsync(Guid id);
        Task<PharmacyService?> GetByIdForPharmacyAsync(Guid id, string pharmacyUserId);
        Task<(List<PharmacyService> items, int totalCount)> SearchAsync(string? query, string? pharmacyUserId, bool activeOnly, int page, int pageSize);
        Task<(List<PharmacyService> items, int totalCount)> GetByPharmacyAsync(string pharmacyUserId, int page, int pageSize);
        Task<bool> DeleteAsync(Guid id, string pharmacyUserId);
        Task SaveChangesAsync();
    }
}
