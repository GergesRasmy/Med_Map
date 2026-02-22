using System.ComponentModel.DataAnnotations;

namespace Med_Map.Models
{
    public class ApplicationUser:IdentityUser
    {
        public string? AvatarUrl { get; set; }
        [Required]
        public bool IsActive { get; set; }

        #region Navigation Properties
        public virtual Customer? Customer { get; set; }
        public virtual Pharmacy? Pharmacy { get; set; }
        #endregion
    }
}
