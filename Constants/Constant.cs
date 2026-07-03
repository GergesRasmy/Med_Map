namespace Med_Map.Constants
{
    public class Constant
    {
        public const double tokenExpirationTime = 1000000;// in Hours, Approx 114 years, effectively making tokens non-expiring for testing purposes. Adjust as needed for production.
        public const double OtpExpirationTime = 5; // in Minutes
        public const int OtpMaxAttempts = 5; // failed verification attempts before an OTP is locked out
        public const int OtpResendCooldownSeconds = 60; // minimum wait between OTP generations for the same user/purpose
        public const int PageSize = 10;

        // platform/order fees (in the platform's currency) — surfaced by the cart-validation endpoint
        public const decimal AppFee = 5.00m;             // flat platform service fee per order
        public const decimal CashOnDeliveryFee = 15.00m; // extra fee charged when paying cash on delivery
        public const decimal OnlineFee = 7.00m;          // extra fee charged for online (card) payments

        // pending online order expiry — orders unpaid longer than this are auto-cancelled and stock is restored
        public const int PendingOrderExpiryMinutes = 10;

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

        public static class UploadFolders
        {
            public const string CustomerAvatars  = "Customer_Avatars";
            public const string NationalIds      = "National_Ids";
            public const string PharmacyLicenses = "Pharmacy_Licenses";
            public const string MedicineImages   = "Medicine_Images";

            // disk folder → API route segment
            public static readonly IReadOnlyDictionary<string, string> ToApiRoute =
                new Dictionary<string, string>
                {
                    { CustomerAvatars,  "avatars"        },
                    { NationalIds,      "national-ids"   },
                    { PharmacyLicenses, "licenses"       },
                    { MedicineImages,   "medicine-images" },
                };

            // API route segment → disk folder (reverse of above)
            public static readonly IReadOnlyDictionary<string, string> FromApiRoute =
                new Dictionary<string, string>
                {
                    { "avatars",         CustomerAvatars  },
                    { "national-ids",    NationalIds      },
                    { "licenses",        PharmacyLicenses },
                    { "medicine-images", MedicineImages   },
                };
        }
    }
}
