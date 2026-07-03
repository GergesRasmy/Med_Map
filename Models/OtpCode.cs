using System.ComponentModel.DataAnnotations.Schema;

namespace Med_Map.Models
{
    public enum OtpPurpose
    {
        Registration,
        PasswordReset
    }

    public class OtpCode
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // IdentityUser IDs are strings by default
        [Required]
        public string UserId { get; set; }

        [ForeignKey("UserId")]
        public virtual ApplicationUser User { get; set; }

        [Required]
        [StringLength(6)]
        public string Code { get; set; }

        [Required]
        public Guid SessionId { get; set; } = Guid.NewGuid();

        public OtpPurpose Purpose { get; set; } = OtpPurpose.Registration;
        public int AttemptCount { get; set; } = 0;

        public DateTime ExpiresAt { get; set; }
        public bool IsUsed { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
