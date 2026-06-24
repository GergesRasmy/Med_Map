namespace Med_Map.Repositories.PharmacyRepos
{
    public class PharmacyServiceRepository : IPharmacyServiceRepository
    {
        private readonly Mm_Context _context;

        public PharmacyServiceRepository(Mm_Context context)
        {
            _context = context;
        }

        public async Task AddAsync(PharmacyService service)
        {
            await _context.PharmacyServices.AddAsync(service);
            await _context.SaveChangesAsync();
        }

        public async Task<PharmacyService?> GetByIdAsync(Guid id)
        {
            return await _context.PharmacyServices
                .AsNoTracking()
                .Include(s => s.Pharmacy).ThenInclude(p => p.ActiveProfile)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<PharmacyService?> GetByIdForPharmacyAsync(Guid id, string pharmacyUserId)
        {
            return await _context.PharmacyServices
                .FirstOrDefaultAsync(s => s.Id == id && s.PharmacyUserId == pharmacyUserId);
        }

        public async Task<(List<PharmacyService> items, int totalCount)> SearchAsync(
            string? query, string? pharmacyUserId, bool activeOnly, int page, int pageSize)
        {
            var q = _context.PharmacyServices
                .AsNoTracking()
                .Include(s => s.Pharmacy).ThenInclude(p => p.ActiveProfile)
                .Where(s => s.Pharmacy.ActiveProfileId != null);

            if (activeOnly)
                q = q.Where(s => s.IsActive);

            if (!string.IsNullOrWhiteSpace(query))
                q = q.Where(s =>
                    EF.Functions.Like(s.Name, $"%{query}%") ||
                    EF.Functions.Like(s.Description, $"%{query}%"));

            if (!string.IsNullOrWhiteSpace(pharmacyUserId))
                q = q.Where(s => s.PharmacyUserId == pharmacyUserId);

            var totalCount = await q.CountAsync();
            var items = await q
                .OrderBy(s => s.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<(List<PharmacyService> items, int totalCount)> GetByPharmacyAsync(
            string pharmacyUserId, int page, int pageSize)
        {
            var q = _context.PharmacyServices
                .AsNoTracking()
                .Where(s => s.PharmacyUserId == pharmacyUserId);

            var totalCount = await q.CountAsync();
            var items = await q
                .OrderBy(s => s.Name)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<bool> DeleteAsync(Guid id, string pharmacyUserId)
        {
            var service = await _context.PharmacyServices
                .FirstOrDefaultAsync(s => s.Id == id && s.PharmacyUserId == pharmacyUserId);

            if (service == null) return false;

            _context.PharmacyServices.Remove(service);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
