using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using NetTopologySuite.Geometries;

namespace Med_Map.Models
{

    public class Pharmacy
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MinLength(3, ErrorMessage = "Minimum length is 3")]
        [MaxLength(30, ErrorMessage = "Maximum length is 30")]
        public string PharmacyName { get; set; }
        [Required]
        public string LicenseNumber { get; set; }
        [Required]
        [RegularExpression(@"(\-?\d+(\.\d+)?),\s*(\-?\d+(\.\d+)?)",ErrorMessage ="Coordinates are in wrong format")]
        public Point Location { get; set; }
        [Required]
        [RegularExpression(@"^(?:0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Invalid time format (HH:mm).")]
        public TimeSpan OpeningTime { get; set; }
        [Required]
        [RegularExpression(@"^(?:0[0-9]|1[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Invalid time format (HH:mm).")]
        public TimeSpan CloseingTime { get; set; }
        [Required]
        public bool Is24Hours { get; set; }
        [Required]
        [Range(0, 5, ErrorMessage = "Rating must be between 0 and 5")]
        public double Rating { get; set; }


    }
}
