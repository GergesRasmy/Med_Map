namespace Med_Map.Repositories.MedicineRepos
{
    public interface IMedicineRepository
    {
        Task InsertAsync(MedicineMaster medicine);
        Task<MedicineMaster?> GetByIdAsync(string id);
        Task SaveChangesAsync();
    }
}
