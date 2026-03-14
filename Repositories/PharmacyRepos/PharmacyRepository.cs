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

        public async Task InsertAsync(Pharmacy pharmacy)
        {
            _context.Pharmacy.Add(pharmacy);
            await SaveChangesAsync();
        }

        public async Task<Pharmacy?> GetByIdAsync(string id)
        {
            var pharmacy = await _context.Pharmacy.FirstOrDefaultAsync(p => p.ApplicationUserId == id);
            if (pharmacy == null) return null;
            await _context.Entry(pharmacy).Collection(p => p.Documents).LoadAsync();
            await _context.Entry(pharmacy).Collection(p => p.PhoneNumbers).LoadAsync();
            return pharmacy;
        }
        public async Task<List<Pharmacy>> GetByNameAsync(string name)
        {
            string normalizedSearch = name.ToUpper();

            return await _context.Pharmacy
             .Include(p => p.PhoneNumbers)
             .Include(p => p.User) 
             .Where(p =>
                 p.doctorName.ToUpper().Contains(normalizedSearch) ||
                 p.User.NormalizedUserName.Contains(normalizedSearch)) 
             .ToListAsync();
        }
        public async Task<List<Pharmacy>> GetNearestPharmacyAsync(double latitude, double longitude, double radiusInMeters)
        {
            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            var myLocation = geometryFactory.CreatePoint(new Coordinate(longitude, latitude));

            var pharmacies = await _context.Pharmacy
                .Include(p => p.PhoneNumbers)
                .Where(p => p.Location != null && p.Location.Distance(myLocation) <= (radiusInMeters))
                .OrderBy(p => p.Location.Distance(myLocation))
                .ToListAsync();
            return pharmacies;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
