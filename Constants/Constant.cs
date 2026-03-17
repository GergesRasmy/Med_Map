namespace Med_Map.Constants
{
    public class Constant
    {
        public const double tokenExpirationTime = 1000000;// in Hours, Approx 114 years, effectively making tokens non-expiring for testing purposes. Adjust as needed for production.
        public const double OtpExpirationTime = 5; // in Minutes
        public const int PageSize = 10;

        // file upload settings
        public static readonly Dictionary<string, string> AllowedMimeTypes = new(StringComparer.OrdinalIgnoreCase)
        {
            { ".jpg", "image/jpeg" },{ ".jpeg", "image/jpeg" },
            { ".png", "image/png" },{ ".webp", "image/webp" },
            { ".heic", "image/heif" },{ ".heif", "image/heif" },
            { ".tiff", "image/tiff" },{ ".tif", "image/tiff" },
            { ".gif", "image/gif" },{ ".bmp", "image/bmp" }
        };
        public const long MaxFileSize = 5 * 1024 * 1024; // 5 MB
    }
}
