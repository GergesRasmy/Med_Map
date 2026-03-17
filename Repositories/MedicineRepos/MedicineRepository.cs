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
        public async Task<(List<MedicineMaster> items, int totalCount)> GetAllMedicineAsync(int page, int pageSize = 10)
        {
            var query = _context.MedicineMaster.AsNoTracking();

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
        public async Task<MedicineMaster?> GetByIdAsync(string id)
        {
            return await _context.MedicineMaster.FirstOrDefaultAsync(c => c.Id == Guid.Parse(id));
        }
        public async Task<(List<MedicineMaster>? items, int totalCount)> GetByTradeNameAsync(string tradeName, int page, int pageSize = 10)
        {
            if (string.IsNullOrWhiteSpace(tradeName))
                return (new List<MedicineMaster>(), 0);

            var query = _context.MedicineMaster
                .Where(m => m.TradeName.Contains(tradeName))
                .AsNoTracking();

            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
        public async Task UpdateAsync(MedicineMaster existing, UpdateMedicineDTO dto)
        {
            if (dto.tradeName != null)
                existing.TradeName = dto.tradeName;
            if (dto.genericName != null)
                existing.GenericName = dto.genericName;
            if (dto.price != null)
                existing.Price = dto.price.Value;
            if (dto.isRestricted != null)
                existing.IsRestricted = dto.isRestricted.Value;
            if (dto.manufacturer != null)
                existing.Manufacturer = dto.manufacturer;

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
