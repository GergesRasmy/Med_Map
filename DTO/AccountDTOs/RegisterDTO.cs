namespace Med_Map.DTO.AccountDTOs
{
    public class RegisterDTO
    {
        [Required]
        [MinLength(3, ErrorMessage = "Minimum length is 3")]
        [MaxLength(30, ErrorMessage = "Maximum length is 30")]
        public string userName { get; set; }
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string email { get; set; }
        [Required]
        public string role { get; set; }
        public string password { get; set; }
    }
}
