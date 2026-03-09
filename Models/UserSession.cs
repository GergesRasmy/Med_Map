using System.ComponentModel.DataAnnotations.Schema;

namespace Med_Map.Models
{
    public class UserSession
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid(); // This is the 'sid' (Session ID)

        public string? JwtId { get; set; } // This is the 'jti' claim

        public bool IsActive { get; set; }

        public DateTime ExpiresAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [Required]
        public string UserId { get; set; }

        [ForeignKey(nameof(UserId))]
        public virtual ApplicationUser User { get; set; }
    }
}
