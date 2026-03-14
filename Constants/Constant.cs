namespace Med_Map.Constants
{
    public class Constant
    {
        public const double tokenExpirationTime = 1000000;// in Hours, Approx 114 years, effectively making tokens non-expiring for testing purposes. Adjust as needed for production.
        public const double OtpExpirationTime = 5; // in Minutes

        // file upload settings
        public static readonly HashSet<string> AllowedExtensions =
                new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".webp" };
        public const long MaxFileSize = 5 * 1024 * 1024; // 5 MB
    }
}
