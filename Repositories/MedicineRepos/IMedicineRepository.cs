namespace Med_Map.Repositories.MedicineRepos
{
    public interface IMedicineRepository
    {
        Task InsertAsync(MedicineMaster medicine);
        Task<List<MedicineMaster>?> GetAllMedicineAsync();
        Task<bool> ExistsAsync(string tradeName, string? excludeId = null);
        Task<MedicineMaster?> GetByIdAsync(string id);
        Task<List<MedicineMaster>?> GetByTradeNameAsync(string tradeName);
        Task UpdateAsync(MedicineMaster medicine);
        Task DeleteAsync(MedicineMaster medicine);

        Task SaveChangesAsync();
    }
}
