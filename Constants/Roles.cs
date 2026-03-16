namespace Med_Map.Constants
{
    public class RoleConstants
    {
        public static class Names
        {
            public const string Pharmacy = "Pharmacy";
            public const string Customer = "Customer";
            public const string Admin = "Admin";
        }

        public static readonly HashSet<string> All = new HashSet<string>
        {
            Names.Pharmacy,
            Names.Customer,
            Names.Admin
        };
    }
}
