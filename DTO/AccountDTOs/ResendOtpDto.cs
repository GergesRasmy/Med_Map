using System.Text.Json.Serialization;

namespace Med_Map.DTO.AccountDTOs
{
    public class ResendOtpDto
    {
        [Required]
        [RegularExpression(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", ErrorMessage = "Please enter a valid email address.")]
        public string email { get; set; }
    }
}
