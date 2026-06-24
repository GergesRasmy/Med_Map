using NetTopologySuite;

namespace Med_Map.Seeders
{
    public static class PharmacySeeder
    {
        private const string Password          = "#BBbb123";
        private const int    InventoryPerPharm = 8;

        private static readonly (
            string Email, string DisplayName,
            string PharmacyName, string LicenseNo,
            string Address, double Lat, double Lon,
            string Phone,
            TimeSpan Opening, TimeSpan Closing,
            bool Is24Hours, bool HasDelivery,
            decimal DeliveryFee, double DeliveryRadiusKm,
            string IdFile, string LicenseFile)[] PharmacyData =
        {
            (
                "p1@g.com", "El-Ezaby Pharmacy",
                "El-Ezaby Pharmacy", "PH-2024-CAI-001",
                "12 El Nozha Street, Heliopolis, Cairo", 30.0875, 31.3243,
                "01012345678",
                new TimeSpan(8, 0, 0), new TimeSpan(22, 0, 0),
                false, true,
                15m, 5.0,
                "p1_id.jpg", "p1_license.jpg"
            ),
            (
                "p2@g.com", "Seif Pharmacy",
                "Seif Pharmacy", "PH-2024-CAI-002",
                "8 Hassan Sabri Street, Zamalek, Cairo", 30.0622, 31.2214,
                "01123456789",
                new TimeSpan(9, 0, 0), new TimeSpan(23, 0, 0),
                false, false,
                0m, 0.0,
                "p2_id.jpg", "p2_license.jpg"
            ),
        };

        public static async Task SeedAsync(Mm_Context context, UserManager<ApplicationUser> userManager, IWebHostEnvironment env)
        {
            if (await context.Pharmacy.AnyAsync())
                return;

            var factory    = NtsGeometryServices.Instance.CreateGeometryFactory(srid: 4326);
            var medicines  = await context.MedicineMaster.Take(InventoryPerPharm).ToListAsync();
            var expiryDate = DateOnly.FromDateTime(DateTime.UtcNow.AddYears(2));

            foreach (var p in PharmacyData)
            {
                // 1 — ApplicationUser
                var user = new ApplicationUser
                {
                    Id             = Guid.NewGuid().ToString(),
                    UserName       = p.Email,
                    Email          = p.Email,
                    displayName    = p.DisplayName,
                    EmailConfirmed = true,
                    IsActive       = true,
                };
                var result = await userManager.CreateAsync(user, Password);
                if (!result.Succeeded)
                    throw new Exception($"PharmacySeeder: failed to create {p.Email} — {string.Join(", ", result.Errors.Select(e => e.Description))}");

                await userManager.AddToRoleAsync(user, RoleConstants.Names.Pharmacy);

                // 2 — Copy this pharmacy's document images and get their URLs
                var (idUrl, licenseUrl) = CopyDocuments(env, p.IdFile, p.LicenseFile);

                // 3 — PharmacyProfile with documents and phone in one shot
                var profile = new PharmacyProfile
                {
                    PharmacyName  = p.PharmacyName,
                    LicenseNumber = p.LicenseNo,
                    address       = p.Address,
                    Location      = factory.CreatePoint(new Coordinate(p.Lon, p.Lat)),
                    OpeningTime   = p.Opening,
                    ClosingTime   = p.Closing,
                    Is24Hours        = p.Is24Hours,
                    HaveDelivary     = p.HasDelivery,
                    DeliveryFee      = p.DeliveryFee,
                    DeliveryRadiusKm = p.DeliveryRadiusKm,
                    Rating           = 4.5,
                    Documents = new List<PharmacyDocument>
                    {
                        new() { FileUrl = idUrl,      Type = DocumentType.NationalId },
                        new() { FileUrl = licenseUrl, Type = DocumentType.PharmacyLicense },
                    },
                    PhoneNumbers = new List<PharmacyPhoneNumbers>
                    {
                        new() { Number = p.Phone },
                    },
                };

                context.PharmacyProfile.Add(profile);
                await context.SaveChangesAsync();

                // 4 — Pharmacy row linking user → active profile
                context.Pharmacy.Add(new Pharmacy
                {
                    ApplicationUserId = user.Id,
                    ActiveProfileId   = profile.Id,
                });
                await context.SaveChangesAsync();

                // 4b — Wallet (mirrors what activateProfile endpoint does)
                context.Wallet.Add(new Wallet
                {
                    PharmacyUserId = user.Id,
                    CurrentBalance = 0,
                    TotalEarnings  = 0,
                    Currency       = CurrencyType.EGP,
                });
                await context.SaveChangesAsync();

                // 5 — Inventory: first N seeded medicines at their registered price
                foreach (var med in medicines)
                {
                    context.PharmacyInventory.Add(new PharmacyInventory
                    {
                        PharmacyUserId = user.Id,
                        MedicineId     = med.Id,
                        Price             = med.Price,
                        StockQuantity     = 50,
                        ExpiryDate        = expiryDate,
                    });
                }
                await context.SaveChangesAsync();

                // 6 — Services catalog
                var services = PharmacyServices(user.Id);
                context.PharmacyServices.AddRange(services);
                await context.SaveChangesAsync();
            }
        }

        private static List<PharmacyService> PharmacyServices(string pharmacyUserId) =>
        [
            new() { PharmacyUserId = pharmacyUserId, Name = "Blood Pressure Measurement",         Price = 30m,  Description = "Quick and accurate blood pressure check using a digital sphygmomanometer." },
            new() { PharmacyUserId = pharmacyUserId, Name = "Blood Glucose Test",                 Price = 40m,  Description = "Fingertip blood glucose reading using a calibrated glucometer." },
            new() { PharmacyUserId = pharmacyUserId, Name = "Temperature Measurement",            Price = 20m,  Description = "Body temperature check using a digital thermometer." },
            new() { PharmacyUserId = pharmacyUserId, Name = "Weight & BMI Calculation",           Price = 25m,  Description = "Weigh-in and Body Mass Index calculation with dietary guidance." },
            new() { PharmacyUserId = pharmacyUserId, Name = "Blood Oxygen Saturation (SpO2)",     Price = 30m,  Description = "Pulse oximetry reading to measure blood oxygen saturation level." },
            new() { PharmacyUserId = pharmacyUserId, Name = "Intramuscular / IV Injection",       Price = 80m,  Description = "Administration of prescribed IM or IV injections by a licensed nurse." },
            new() { PharmacyUserId = pharmacyUserId, Name = "Wound Dressing & Drug Application", Price = 70m,  Description = "Professional wound cleaning, dressing change, and topical drug application." },
            new() { PharmacyUserId = pharmacyUserId, Name = "Drug Interactions & Dosing Advice",  Price = 50m,  Description = "Pharmacist review of your current medications for interactions and correct dosing." },
            new() { PharmacyUserId = pharmacyUserId, Name = "Prescription Interpretation",        Price = 45m,  Description = "Pharmacist explains your doctor's prescription and answers questions about your medications." },
        ];

        private static (string idUrl, string licenseUrl) CopyDocuments(IWebHostEnvironment env, string idFile, string licenseFile)
        {
            var dataDir = Path.Combine(AppContext.BaseDirectory, "Seeders", "Data");
            var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
            var destDir = Path.Combine(webRoot, "uploads", "Pharmacy_Documents");
            Directory.CreateDirectory(destDir);

            string Copy(string fileName)
            {
                var src  = Path.Combine(dataDir, fileName);
                var dest = Path.Combine(destDir, fileName);
                if (File.Exists(src) && !File.Exists(dest))
                    File.Copy(src, dest);
                return $"/uploads/Pharmacy_Documents/{fileName}";
            }

            return (Copy(idFile), Copy(licenseFile));
        }
    }
}
