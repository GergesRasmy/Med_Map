namespace Med_Map.Repositories.MedicineRepos
{
    public class MedicineRepository : IMedicineRepository
    {
        private readonly Mm_Context _context;

        public MedicineRepository(Mm_Context context)
        {
            _context = context;
        }
        public async Task InsertAsync(MedicineMaster medicine)
        {
            _context.MedicineMaster.Add(medicine);
            await SaveChangesAsync();
        }
        public async Task<List<MedicineMaster>?> GetAllMedicineAsync()
        {
            return await _context.MedicineMaster.ToListAsync();
        }
        public async Task<MedicineMaster?> GetByIdAsync(string id)
        {
            return await _context.MedicineMaster.FirstAsync(c => c.Id == Guid.Parse(id));
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
