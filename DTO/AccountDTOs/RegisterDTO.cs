namespace Med_Map.DTO.AccountDTOs
{
    public class RegisterDTO
    {
        [Required]
        [MaxLength(100)]
        public string displayName { get; set; }
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string email { get; set; }
        [Required]
        public string role { get; set; }
        public string password { get; set; }
    }
}
