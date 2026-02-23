namespace Med_Map.DTO
{
    public class CustomerRegisterDTO
    {
        [Required]
        [MinLength(3, ErrorMessage = "Minimum length is 3")]
        [MaxLength(30, ErrorMessage = "Maximum length is 30")]
        public string TraddeName { get; set; }
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }
        [Required]
        [RegularExpression(@"^(\+201|01)[0125][0-9]{8}$", ErrorMessage = "Invalid phone number.")]
        public string Phone { get; set; }
        [Display(Name = "Password")]
        public string PasswordHash { get; set; }
        public bool TermConditions { get; set; }
    }
}
