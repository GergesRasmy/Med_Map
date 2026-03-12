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
        public async Task<bool> ExistsAsync(string tradeName)
        {
            return await _context.MedicineMaster
                .AnyAsync(m => m.TradeName.ToLower() == tradeName.ToLower());
        }
        public async Task<List<MedicineMaster>?> GetAllMedicineAsync()
        {
            return await _context.MedicineMaster.ToListAsync();
        }
        public async Task<MedicineMaster?> GetByIdAsync(string id)
        {
            return await _context.MedicineMaster.FirstAsync(c => c.Id == Guid.Parse(id));
        }
        public async Task<List<MedicineMaster>?> GetByTradeNameAsync(string tradeName)
        {
            if (string.IsNullOrWhiteSpace(tradeName))
                return new List<MedicineMaster>();

            return await _context.MedicineMaster
                .Where(m => m.TradeName.Contains(tradeName))
                .ToListAsync();
        }
        public async Task UpdateAsync(MedicineMaster medicine)
        {
            if (medicine != null)
            {
                _context.MedicineMaster.Update(medicine);
                await SaveChangesAsync();
            }
        }
        public async Task DeleteAsync(string id)
        {
            var medicine = await GetByIdAsync(id);
            if (medicine != null)
            {
                _context.MedicineMaster.Remove(medicine);
                await SaveChangesAsync();
            }
        }
        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
