namespace Med_Map.Repositories.MedicineRepos
{
    public interface IMedicineRepository
    {
        Task InsertAsync(MedicineMaster medicine);
        Task UpdateAsync(MedicineMaster existing, UpdateMedicineDTO dto);
        Task<(List<MedicineMaster> items, int totalCount)> GetAllMedicineAsync(int page, int pageSize);
        Task<bool> ExistsAsync(string tradeName, string? excludeId = null);
        Task<MedicineMaster?> GetByIdAsync(string id);
        Task<(List<MedicineMaster>? items, int totalCount)> GetByTradeNameAsync(string tradeName, int page, int pageSize);
        Task DeleteAsync(MedicineMaster medicine);

        Task SaveChangesAsync();
    }
}
