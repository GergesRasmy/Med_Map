using System.ComponentModel.DataAnnotations;

namespace Med_Map.DTO.AccountDTOs
{
    public class ChangePasswordDTO
    {
        [Required]
        public string currentPassword { get; set; } = string.Empty;

        [Required]
        [MinLength(8)]
        public string newPassword { get; set; } = string.Empty;

        [Required]
        [Compare(nameof(newPassword), ErrorMessage = "Passwords do not match.")]
        public string confirmNewPassword { get; set; } = string.Empty;
    }
}
