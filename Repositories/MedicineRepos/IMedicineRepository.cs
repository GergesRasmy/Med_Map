namespace Med_Map.Repositories.MedicineRepos
{
    public interface IMedicineRepository
    {
        Task InsertAsync(MedicineMaster medicine);
        Task<List<MedicineMaster>?> GetAllMedicineAsync();
        Task<MedicineMaster?> GetByIdAsync(string id);
        Task SaveChangesAsync();
    }
}
