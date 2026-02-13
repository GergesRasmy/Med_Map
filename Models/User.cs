using System.ComponentModel.DataAnnotations;

namespace Med_Map.Models
{
    /*
     Id (Primary Key, Guid)
    UserName (String)
    Email (String, Unique)
    PasswordHash (String)
    Role (Enum: Admin, Pharmacy, Customer)
    AvatarUrl (String, Nullable)
    IsActive (Boolean)
     */
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MinLength(3,ErrorMessage ="Minimum length is 3")]
        [MaxLength(30, ErrorMessage = "Maximum length is 30")]
        public string UserName { get; set; }
        [Required]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; }
        [Required]
        [RegularExpression("Admin|Pharmacy|Customer", ErrorMessage = "Role doesn't exist")]
        public List<String> Role { get; set; } = ["Admin", "Pharmacy", "Customer"];
        public string? AvatarUrl { get; set; }
        public bool IsActive { get; set; }
    }
}
