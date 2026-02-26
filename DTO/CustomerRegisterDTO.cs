using System.Text.Json.Serialization;

namespace Med_Map.DTO
{
    public class CustomerRegisterDTO
    {
        [Required]
        [MinLength(3, ErrorMessage = "Minimum length is 3")]
        [MaxLength(30, ErrorMessage = "Maximum length is 30")]
        public string userName { get; set; }
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string email { get; set; }
        [Required]
        [RegularExpression(@"^(\+201|01)[0125][0-9]{8}$", ErrorMessage = "Invalid phone number.")]
        public string phoneNumber { get; set; }
        [JsonPropertyName("password")]
        public string PasswordHash { get; set; }
        public bool TermConditions { get; set; }
    }
}
