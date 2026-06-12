namespace Med_Map.Seeders
{
    public static class AdminSeeder
    {
        private const string AdminEmail = "admin@medmap.dev";
        private const string AdminPassword = "Admin@123456";
        private const string AdminDisplayName = "Admin";

        public static async Task SeedAsync(UserManager<ApplicationUser> userManager)
        {
            if (await userManager.FindByEmailAsync(AdminEmail) != null)
                return;

            var admin = new ApplicationUser
            {
                Id = Guid.NewGuid().ToString(),
                UserName = AdminEmail,
                Email = AdminEmail,
                displayName = AdminDisplayName,
                EmailConfirmed = true,
            };

            var result = await userManager.CreateAsync(admin, AdminPassword);
            if (!result.Succeeded)
                throw new Exception($"AdminSeeder: failed to create admin — {string.Join(", ", result.Errors.Select(e => e.Description))}");

            await userManager.AddToRoleAsync(admin, RoleConstants.Names.Admin);
        }
    }
}
