using System.Text.Json.Serialization;

namespace Med_Map.DTO.AccountDTOs
{
    public class PharmacyRegisterDTO
    {
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string email { get; set; }
        public string password { get; set; }

        [Required]
        [MinLength(3, ErrorMessage = "Minimum length is 3")]
        [MaxLength(30, ErrorMessage = "Maximum length is 30")]
        public string pharmacyName { get; set; }
        [Required]
        [RegularExpression(@"^(\+201|01)[0125][0-9]{8}$", ErrorMessage = "Invalid phone number.")]
        public List<string> pharmacyPhones { get; set; } = new List<string>();

        [Required]
        [MinLength(3, ErrorMessage = "Minimum length is 3")]
        [MaxLength(30, ErrorMessage = "Maximum length is 30")]
        public string doctorName { get; set; }
        [Required]
        [RegularExpression(@"^(\+201|01)[0125][0-9]{8}$", ErrorMessage = "Invalid phone number.")]
        public string doctorPhoneNumber { get; set; }

        public string? address { get; set; }
        [Required]
        public double latitude { get; set; }
        [Required]
        public double longitude { get; set; }


        [Required]
        [Range(typeof(TimeSpan), "00:00", "23:59", ErrorMessage = "opening time must be between 00:00 and 23:59")]
        public TimeSpan openingTime { get; set; }
        [Required]
        [Range(typeof(TimeSpan), "00:00", "23:59", ErrorMessage = "Closing time must be between 00:00 and 23:59")]
        public TimeSpan closingTime { get; set; }
        
        [Required(ErrorMessage = "National ID image is required")]
        public List<IFormFile> nationalIds { get; set; } = new List<IFormFile>();
        [Required]
        public string licenseNumber { get; set; }
        [Required(ErrorMessage = "License document image is required")]
        public List<IFormFile> licenseImages { get; set; } = new List<IFormFile>();
        [Required]
        public bool is24Hours { get; set; }
        [Required]
        public bool delivaryAvailability { get; set; }


    }
}
