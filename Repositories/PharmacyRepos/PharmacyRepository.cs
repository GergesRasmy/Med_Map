using NetTopologySuite;

namespace Med_Map.Repositories.PharmacyRepos
{
    public class PharmacyRepository: IPharmacyRepository
    {
        private readonly Mm_Context _context;
        private readonly ILogger<PharmacyRepository> logger;

        public PharmacyRepository(Mm_Context context, ILogger<PharmacyRepository> logger)
        {
            _context = context;
            this.logger = logger;
        }
        public async Task SaveToPendingAsync(string userId, PharmacyProfile newProfile)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var pharmacy = await _context.Pharmacy
                    .Include(p => p.PendingProfile).ThenInclude(pp => pp!.Documents)
                    .Include(p => p.PendingProfile).ThenInclude(pp => pp!.PhoneNumbers)
                    .FirstOrDefaultAsync(p => p.ApplicationUserId == userId);

                if (pharmacy == null) throw new Exception("Pharmacy not found");

                // 1. Wipe the old Pending Profile completely
                if (pharmacy.PendingProfile != null)
                {
                    _context.Set<PharmacyDocument>().RemoveRange(pharmacy.PendingProfile.Documents);
                    _context.Set<PharmacyPhoneNumbers>().RemoveRange(pharmacy.PendingProfile.PhoneNumbers);
                    _context.PharmacyProfile.Remove(pharmacy.PendingProfile);

                    // Sync the deletion before adding new data to avoid unique constraint hits
                    await _context.SaveChangesAsync();
                }

                // 2. Prepare the new profile graph
                newProfile.Id = Guid.Empty; // Ensure it's 0000...
                foreach (var doc in newProfile.Documents) doc.Id = Guid.Empty;
                foreach (var phone in newProfile.PhoneNumbers) phone.Id = Guid.Empty;

                // 3. Add to context and Link to the Pharmacy
                _context.PharmacyProfile.Add(newProfile);
                pharmacy.PendingProfile = newProfile;

                // 4. Save everything
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                logger.LogError(ex, "SaveToPendingAsync failed: {Message}", ex.Message);
                throw;
            }
        }
        public async Task<(List<Pharmacy> items, int totalCount)> GetAllPharmaciesPaginatedAsync(int page, int pageSize)
        {
            var query = _context.Pharmacy.AsNoTracking()
                .Include(p => p.User)
                .Include(p => p.ActiveProfile).ThenInclude(ap => ap!.Documents)
                .Include(p => p.ActiveProfile).ThenInclude(ap => ap!.PhoneNumbers)
                .Include(p => p.PendingProfile).ThenInclude(pp => pp!.Documents)
                .Include(p => p.PendingProfile).ThenInclude(pp => pp!.PhoneNumbers);

            var totalCount = await query.CountAsync();
            var items = await query
                .OrderBy(p => p.ApplicationUserId)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
        public async Task InsertAsync(Pharmacy pharmacy)
        {
            await _context.Pharmacy.AddAsync(pharmacy);
            await _context.SaveChangesAsync();
        }
        public async Task UpdateInstantFieldsAsync(string userId, PharmacyUpdateDTO model)
        {
            var pharmacy = await _context.Pharmacy
                .Include(p => p.ActiveProfile)
                .FirstOrDefaultAsync(p => p.ApplicationUserId == userId);

            if (pharmacy?.ActiveProfile == null) return;

            if (model.deliveryAvailability.HasValue)
                pharmacy.ActiveProfile.HaveDelivary = model.deliveryAvailability.Value;
            if (model.is24Hours.HasValue)
                pharmacy.ActiveProfile.Is24Hours = model.is24Hours.Value;
            if (model.openingTime.HasValue)
                pharmacy.ActiveProfile.OpeningTime = model.openingTime.Value;
            if (model.closingTime.HasValue)
                pharmacy.ActiveProfile.ClosingTime = model.closingTime.Value;

            await _context.SaveChangesAsync();
        }
        public async Task<bool> ActivateProfileAsync(string userId)
        {
            var pharmacy = await _context.Pharmacy
                .Include(p => p.ActiveProfile)
                .Include(p => p.PendingProfile)
                .FirstOrDefaultAsync(p => p.ApplicationUserId == userId);

            if (pharmacy == null || pharmacy.PendingProfile == null) return false;

            pharmacy.ActiveProfile = pharmacy.PendingProfile;
            pharmacy.ActiveProfileId = pharmacy.PendingProfile!.Id;
            pharmacy.PendingProfile = null;
            pharmacy.PendingProfileId = null;


            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<Pharmacy?> GetByIdAsync(string id)
        {
            return await _context.Pharmacy.AsNoTracking()
                                 .Include(p => p.ActiveProfile)
                                     .ThenInclude(ap => ap!.Documents)
                                 .Include(p => p.ActiveProfile)
                                     .ThenInclude(ap => ap!.PhoneNumbers)
                                 .FirstOrDefaultAsync(p => p.ApplicationUserId == id);
        }
        public async Task<(List<Pharmacy>? items, int totalCount)> GetByNameAsync(string name, int page, int pageSize = 10)
        {
            string normalizedSearch = name.ToUpper();
            var query = _context.Pharmacy.AsNoTracking()
                      .Include(p => p.ActiveProfile)
                          .ThenInclude(ap => ap!.PhoneNumbers)
                      .Include(p => p.User)
                      .Where(p =>
                      (p.ActiveProfile != null && p.ActiveProfile.PharmacyName != null &&
                      p.ActiveProfile.PharmacyName.ToUpper().Contains(normalizedSearch)) ||
                      (p.User != null && p.User.NormalizedUserName != null &&
                      p.User.NormalizedUserName.Contains(normalizedSearch)));

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(p => p.ActiveProfile!.PharmacyName)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        
        }
        public async Task<(List<Pharmacy> items, int totalCount)> GetNearestPharmacyAsync(
        double latitude,
        double longitude,
        double radiusInMeters,
        int page,
        int pageSize = 10)
        {
            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            var myLocation = geometryFactory.CreatePoint(new Coordinate(longitude, latitude));

            var query = _context.Pharmacy.AsNoTracking()
                .Include(p => p.User)
                .Include(p => p.ActiveProfile)
                .Where(p => p.ActiveProfile != null &&
                            p.ActiveProfile.Location.Distance(myLocation) <= radiusInMeters);

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderBy(p => p.ActiveProfile!.Location.Distance(myLocation))
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
        public async Task<Pharmacy?> GetByIdWithPendingAsync(string id)
        {
            return await _context.Pharmacy
                .AsNoTracking()
                 .Include(p => p.ActiveProfile)
                    .ThenInclude(ap => ap!.PhoneNumbers)
                .Include(p => p.ActiveProfile)         
                    .ThenInclude(ap => ap!.Documents)  
                .Include(p => p.PendingProfile)
                    .ThenInclude(pp => pp!.PhoneNumbers)
                .Include(p => p.PendingProfile)
                    .ThenInclude(pp => pp!.Documents)
                .FirstOrDefaultAsync(p => p.ApplicationUserId == id);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
