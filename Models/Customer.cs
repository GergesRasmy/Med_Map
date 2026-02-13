using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace Med_Map.Models
{
    public class Customer
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [RegularExpression(@"^(0[1-9]|[12][0-9]|3[01])/(0[1-9]|1[0-2])/(19|20)\d\d$", ErrorMessage = "Date must be in the format DD/MM/YYYY")]
        public DateOnly BirthDate { get; set; }
        public string? MedicalHistory { get; set; }
        [RegularExpression(@"(\-?\d+(\.\d+)?),\s*(\-?\d+(\.\d+)?)", ErrorMessage = "Coordinates are in wrong format")]
        public List<Point> SavedLocations { get; set; }

        [ForeignKey("User")]
        public int UserId { get; set; }
    }
}
