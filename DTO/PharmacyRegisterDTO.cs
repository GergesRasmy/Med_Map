using System.Text.Json.Serialization;

namespace Med_Map.DTO
{
    public class PharmacyRegisterDTO
    {
        [Required]
        [MinLength(3, ErrorMessage = "Minimum length is 3")]
        [MaxLength(30, ErrorMessage = "Maximum length is 30")]
        public string pharmacyName { get; set; }
        [Required]
        [RegularExpression(@"^(\+201|01)[0125][0-9]{8}$", ErrorMessage = "Invalid phone number.")]
        public string pharmacistPhoneNumber { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string email { get; set; }
        [JsonPropertyName("password")]
        public string PasswordHash { get; set; }
        public bool TermConditions { get; set; }

        public Point location { get; set; }

        [Required]
        [RegularExpression(@"^(\+201|01)[0125][0-9]{8}$", ErrorMessage = "Invalid phone number.")]
        public string pharmacyPhone { get; set; }

        [Required]
        [Range(typeof(TimeSpan), "00:00", "23:59", ErrorMessage = "opening time must be between 00:00 and 23:59")]
        public TimeSpan openingTime { get; set; }
        [Required]
        [Range(typeof(TimeSpan), "00:00", "23:59", ErrorMessage = "Closing time must be between 00:00 and 23:59")]
        public TimeSpan closingTime { get; set; }
        [Required]
        public bool is24Hours { get; set; }
        [Required]
        public string licenseNumber { get; set; }
        [Required(ErrorMessage = "National ID image is required")]
        public IFormFile nationalId { get; set; }

        [Required(ErrorMessage = "License document image is required")]
        public IFormFile licenseImage { get; set; }



    }
}
