using Med_Map.Models.pharmacy;
using NetTopologySuite;

namespace Med_Map.Repositories.PharmacyRepos
{
    public class PharmacyRepository: IPharmacyRepository
    {
        private readonly Mm_Context _context;

        public PharmacyRepository(Mm_Context context)
        {
            _context = context;
        }
        public async Task SaveToPendingAsync(string userId, PharmacyProfile newProfile)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var pharmacy = await _context.Pharmacy
                    .Include(p => p.PendingProfile)
                        .ThenInclude(pp => pp!.Documents)
                    .Include(p => p.PendingProfile)
                        .ThenInclude(pp => pp!.PhoneNumbers)
                    .FirstOrDefaultAsync(p => p.ApplicationUserId == userId);

                if (pharmacy == null) throw new Exception("Pharmacy not found");

                if (pharmacy.PendingProfile != null)
                {
                    var old = pharmacy.PendingProfile;
                    pharmacy.PendingProfile = null;
                    pharmacy.PendingProfileId = null;
                    _context.PharmacyProfille.Remove(old);
                }

                await _context.PharmacyProfille.AddAsync(newProfile);
                pharmacy.PendingProfile = newProfile;
                pharmacy.PendingProfileId = newProfile.Id;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw new Exception("Failed to save");
            }
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

            pharmacy.PendingProfile = null;

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
        public async Task<List<Pharmacy>> GetByNameAsync(string name)
        {
            string normalizedSearch = name.ToUpper();
            return await _context.Pharmacy.AsNoTracking()
                                 .Include(p => p.ActiveProfile)
                                     .ThenInclude(ap => ap!.PhoneNumbers)
                                 .Include(p => p.User)
                                 .Where(p =>
                                     (p.ActiveProfile != null && p.ActiveProfile.PharmacyName != null &&
                                      p.ActiveProfile.PharmacyName.ToUpper().Contains(normalizedSearch)) ||
                                     (p.User != null && p.User.NormalizedUserName != null &&
                                      p.User.NormalizedUserName.Contains(normalizedSearch)))
                                 .ToListAsync();
        }
        public async Task<List<Pharmacy>> GetNearestPharmacyAsync(double latitude, double longitude, double radiusInMeters)
        {
            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            var myLocation = geometryFactory.CreatePoint(new Coordinate(longitude, latitude));

            return await _context.Pharmacy.AsNoTracking()
                .Include(p => p.ActiveProfile)
                .Where(p => p.ActiveProfile != null && p.ActiveProfile.Location.Distance(myLocation) * 111320 <= radiusInMeters)
                .OrderBy(p => p.ActiveProfile!.Location.Distance(myLocation))
                .ToListAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
