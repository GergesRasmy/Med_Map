namespace Med_Map.Seeders
{
    public static class CustomerSeeder
    {
        private const string Password = "#BBbb123";

        private static readonly (string Email, string DisplayName, DateOnly BirthDate, string Address, string? MedicalHistory)[] Customers =
        {
            ("c1@g.com", "Ahmed Hassan",  new DateOnly(1990,  5, 15), "15 Tahrir Square, Downtown Cairo",          "No known allergies"),
            ("c2@g.com", "Sara Mohamed",  new DateOnly(1995,  8, 22), "27 El Nozha Street, Heliopolis, Cairo",     "Asthma"),
        };

        public static async Task SeedAsync(Mm_Context context, UserManager<ApplicationUser> userManager)
        {
            if (await context.Customer.AnyAsync())
                return;

            foreach (var (email, displayName, birthDate, address, medicalHistory) in Customers)
            {
                var user = new ApplicationUser
                {
                    Id             = Guid.NewGuid().ToString(),
                    UserName       = email,
                    Email          = email,
                    displayName    = displayName,
                    EmailConfirmed = true,
                    IsActive       = true,
                };

                var result = await userManager.CreateAsync(user, Password);
                if (!result.Succeeded)
                    throw new Exception($"CustomerSeeder: failed to create {email} — {string.Join(", ", result.Errors.Select(e => e.Description))}");

                await userManager.AddToRoleAsync(user, RoleConstants.Names.Customer);

                context.Customer.Add(new Customer
                {
                    ApplicationUserId = user.Id,
                    BirthDate         = birthDate,
                    address           = address,
                    MedicalHistory    = medicalHistory,
                });
            }

            await context.SaveChangesAsync();
        }
    }
}
