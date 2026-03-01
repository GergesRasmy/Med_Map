using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace Med_Map.Models.customer
{
    public class Customer
    {
        [Key]
        public Guid Id { get; set; }= Guid.NewGuid();
        [Required]
        [Range(typeof(DateOnly), "01/01/1900", "01/01/2026", ErrorMessage = "Date must be in the format DD/MM/YYYY")]
        public DateOnly BirthDate { get; set; }
        public string? MedicalHistory { get; set; }

        // --- Identity Link ---
        [Required]
        public string ApplicationUserId { get; set; } 
        [ForeignKey("ApplicationUserId")]
        public ApplicationUser User { get; set; } 

    }
}
