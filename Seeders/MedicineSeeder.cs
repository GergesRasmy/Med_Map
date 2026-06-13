using System.Text.Json;

namespace Med_Map.Seeders
{
    public static class MedicineSeeder
    {
        private const int Limit = 1000;
        private const string PlaceholderImageUrl = "/uploads/Medicine_Images/placeholder.jpg";

        public static async Task SeedAsync(Mm_Context context, IWebHostEnvironment env)
        {
            if (await context.MedicineMaster.AnyAsync())
                return;

            var dataDir = Path.Combine(AppContext.BaseDirectory, "Seeders", "Data");
            var jsonPath = Path.Combine(dataDir, "extracted_drugs_final.json");
            if (!File.Exists(jsonPath))
                return;

            EnsurePlaceholderImage(dataDir, env);

            var json = await File.ReadAllTextAsync(jsonPath);
            var drugs = JsonSerializer.Deserialize<List<JsonElement>>(json);
            if (drugs == null) return;

            var usedTradeNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var usedRegNos    = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var medicines     = new List<MedicineMaster>();

            foreach (var drug in drugs)
            {
                if (medicines.Count >= Limit) break;

                var details = drug.GetProperty("drug_details");
                var reg = drug.GetProperty("registration_details");
                var generics = drug.GetProperty("generics");
                var packages = drug.GetProperty("packages");

                var tradeName = Str(details, "trade_name");
                if (string.IsNullOrWhiteSpace(tradeName)) continue;
                if (!usedTradeNames.Add(tradeName)) continue;

                var genericNames = generics.EnumerateArray()
                    .Select(g => Str(g, "name"))
                    .Where(n => !string.IsNullOrWhiteSpace(n))
                    .ToList();
                var genericName = genericNames.Count > 0 ? string.Join(", ", genericNames) : tradeName;

                var price = 1m;
                foreach (var pkg in packages.EnumerateArray())
                {
                    if (decimal.TryParse(Str(pkg, "price"), out var parsed) && parsed > 0)
                    {
                        price = parsed;
                        break;
                    }
                }

                var manufacturer = Str(reg, "applicant") is { Length: > 0 } m ? m : "NOT_SET";
                var isRestricted = !string.Equals(Str(reg, "license_status"), "Valid", StringComparison.OrdinalIgnoreCase);

                var regNo = Str(reg, "registration_no");
                if (regNo != null && !usedRegNos.Add(regNo))
                    regNo = null;

                medicines.Add(new MedicineMaster
                {
                    TradeName    = Clip(tradeName, 300),
                    GenericName  = Clip(genericName, 300),
                    Price        = price,
                    ImageUrl     = PlaceholderImageUrl,
                    IsRestricted = isRestricted,
                    Manufacturer = Clip(manufacturer, 300),
                    DosageForm   = Clip(Str(details, "dosage_form"), 100),
                    Strength     = Clip(Str(details, "strength"), 100),
                    Route        = Clip(Str(details, "route"), 50),
                    RegistrationNo = Clip(regNo, 50),
                });
            }

            await context.MedicineMaster.AddRangeAsync(medicines);
            await context.SaveChangesAsync();
        }

        private static void EnsurePlaceholderImage(string dataDir, IWebHostEnvironment env)
        {
            var src = Path.Combine(dataDir, "thumb.jpg");
            if (!File.Exists(src)) return;

            var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
            var destDir = Path.Combine(webRoot, "uploads", "Medicine_Images");
            Directory.CreateDirectory(destDir);
            var dest = Path.Combine(destDir, "placeholder.jpg");
            if (!File.Exists(dest))
                File.Copy(src, dest);
        }

        private static string? Str(JsonElement el, string key) =>
            el.TryGetProperty(key, out var v) && v.ValueKind == JsonValueKind.String
                ? v.GetString()?.Trim()
                : null;

        private static string? Clip(string? value, int max) =>
            value == null ? null : value.Length <= max ? value : value[..max];
    }
}
