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
            if (medicine != null)
            {
                _context.MedicineMaster.Add(medicine);
                await SaveChangesAsync();
            }
        }
        public async Task<bool> ExistsAsync(string tradeName, string? excludeId = null)
        {
            return await _context.MedicineMaster
                .AnyAsync(m => m.TradeName == tradeName && (excludeId == null || m.Id.ToString() != excludeId));
        }
        public async Task<List<MedicineMaster>?> GetAllMedicineAsync()
        {
            return await _context.MedicineMaster.AsNoTracking().ToListAsync();
        }
        public async Task<MedicineMaster?> GetByIdAsync(string id)
        {
            return await _context.MedicineMaster.FirstOrDefaultAsync(c => c.Id == Guid.Parse(id));
        }
        public async Task<List<MedicineMaster>?> GetByTradeNameAsync(string tradeName)
        {
            if (string.IsNullOrWhiteSpace(tradeName))
                return new List<MedicineMaster>();

            return await _context.MedicineMaster
                .Where(m => m.TradeName.Contains(tradeName)).AsNoTracking()
                .ToListAsync();
        }
        public async Task UpdateAsync(MedicineMaster medicine)
        {
            _context.MedicineMaster.Update(medicine);
            await SaveChangesAsync();
        }
        public async Task DeleteAsync(MedicineMaster medicine)
        {
            _context.MedicineMaster.Remove(medicine);
            await SaveChangesAsync();
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
