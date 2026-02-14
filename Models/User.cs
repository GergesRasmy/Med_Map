using Microsoft.EntityFrameworkCore;
using NetTopologySuite.Features;
using System.ComponentModel.DataAnnotations;

namespace Med_Map.Models
{
    public enum UserRole
    {
        Admin,
        Pharmacy,
        Customer
    }
    [Index(nameof(Email), IsUnique = true)]
    public class User
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        [MinLength(3,ErrorMessage ="Minimum length is 3")]
        [MaxLength(30, ErrorMessage = "Maximum length is 30")]
        public string UserName { get; set; }
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }
        [Required]
        [MinLength(8, ErrorMessage = "Password must be atleast 8 characters")]
        [MaxLength(32, ErrorMessage = "Password can't be more than 32 characters")]
        public string PasswordHash { get; set; }
        [Required]
        public UserRole Role { get; set; } 
        public string? AvatarUrl { get; set; }
        [Required]
        public bool IsActive { get; set; }
        [Required]
        [RegularExpression(@"^(\+201|01)[0125][0-9]{8}$", ErrorMessage = "Invalid phone number.")]
        public List<string> PhoneNumbers { get; set; }

        public virtual Customer? Customer { get; set; }
        public virtual Pharmacy? Pharmacy { get; set; }
    }
}
