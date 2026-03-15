using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Med_Map.Models.pharmacy
{
    public class Pharmacy
    {
        // --- Identity Link ---
        [Key, ForeignKey("User")]
        public string ApplicationUserId { get; set; } 
        public ApplicationUser User { get; set; }

        public Guid? ActiveProfileId { get; set; }
        [ForeignKey("ActiveProfileId")]
        public PharmacyProfile? ActiveProfile { get; set; }

        public Guid? PendingProfileId { get; set; }
        [ForeignKey("PendingProfileId")]
        public PharmacyProfile? PendingProfile { get; set; }
    }
}
