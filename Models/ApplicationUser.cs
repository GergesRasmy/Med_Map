namespace Med_Map.Models
{
    public class ApplicationUser:IdentityUser
    {
        public string? displayName { get; set; }
        public string? AvatarUrl { get; set; }
        [Required]
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        #region Navigation Properties
        public virtual Customer? Customer { get; set; }
        public virtual Pharmacy? Pharmacy { get; set; }
        #endregion
    }
}
