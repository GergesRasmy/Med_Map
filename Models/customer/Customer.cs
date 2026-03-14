using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace Med_Map.Models.customer
{
    public class Customer
    {
        [Required]
        [Range(typeof(DateOnly), "01/01/1900", "01/01/2026", ErrorMessage = "Date must be in the format DD/MM/YYYY")]
        public DateOnly BirthDate { get; set; }
        public string? MedicalHistory { get; set; }

        public string? address { get; set; }

        // --- Identity Link ---
        [Key, ForeignKey("User")]
        public string ApplicationUserId { get; set; } 
        public ApplicationUser User { get; set; }

    }
}
