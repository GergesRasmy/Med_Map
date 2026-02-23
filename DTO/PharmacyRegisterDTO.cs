namespace Med_Map.DTO
{
    public class PharmacyRegisterDTO
    {
        [Required]
        [MinLength(3, ErrorMessage = "Minimum length is 3")]
        [MaxLength(30, ErrorMessage = "Maximum length is 30")]
        public string TradeName { get; set; }
        [Required]
        [RegularExpression(@"^(\+201|01)[0125][0-9]{8}$", ErrorMessage = "Invalid phone number.")]
        public string PharmacistPhoneNumber { get; set; }

        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }
        [Display(Name = "Password")]
        public string PasswordHash { get; set; }
        public bool TermConditions { get; set; }

        public Point Location { get; set; }
        [Required]
        [RegularExpression(@"^(\+201|01)[0125][0-9]{8}$", ErrorMessage = "Invalid phone number.")]
        public string PharmacyPhone { get; set; }

        [Required]
        [Range(typeof(TimeSpan), "00:00", "23:59", ErrorMessage = "opening time must be between 00:00 and 23:59")]
        public TimeSpan OpeningTime { get; set; }
        [Required]
        [Range(typeof(TimeSpan), "00:00", "23:59", ErrorMessage = "Closing time must be between 00:00 and 23:59")]
        public TimeSpan ClosingTime { get; set; }
        [Required]
        public bool Is24Hours { get; set; }
        [Required]
        public string LicenseNumber { get; set; }




    }
}
