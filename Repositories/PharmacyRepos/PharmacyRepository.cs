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

                if (pharmacy.PendingProfile != null)
                {
                    var oldProfile = pharmacy.PendingProfile;

                    if (oldProfile.Documents != null)
                        _context.Set<PharmacyDocument>().RemoveRange(oldProfile.Documents);

                    if (oldProfile.PhoneNumbers != null)
                        _context.Set<PharmacyPhoneNumbers>().RemoveRange(oldProfile.PhoneNumbers);

                    _context.PharmacyProfille.Remove(oldProfile);

                    await _context.SaveChangesAsync();
                }

                _context.Entry(newProfile).State = EntityState.Added;

                pharmacy.PendingProfile = newProfile;
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                logger.LogError(ex, "SaveToPendingAsync failed: {Message} | Inner: {Inner}",
                    ex.Message, ex.InnerException?.Message); 
                throw;
            }
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
                .Include(p => p.ActiveProfile)
                .Where(p => p.ActiveProfile != null &&
                            p.ActiveProfile.Location.Distance(myLocation) * 111320 <= radiusInMeters);

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
