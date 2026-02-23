using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Med_Map.Models
{

    public class Pharmacy
    {
        [Key]
        public Guid Id { get; set; }=Guid.NewGuid();
        [Required]
        [MinLength(3, ErrorMessage = "Minimum length is 3")]
        [MaxLength(30, ErrorMessage = "Maximum length is 30")]
        public string PharmacyName { get; set; }
        [Required]
        public string LicenseNumber { get; set; }
        [Required]
        public Point Location { get; set; }
        [Required]
        [Range(typeof(TimeSpan), "00:00", "23:59", ErrorMessage = "opening time must be between 00:00 and 23:59")]
        public TimeSpan OpeningTime { get; set; }
        [Required]
        [Range(typeof(TimeSpan), "00:00", "23:59", ErrorMessage = "Closing time must be between 00:00 and 23:59")]
        public TimeSpan ClosingTime { get; set; }
        [Required]
        public bool Is24Hours { get; set; }
        [Required]
        [Range(0, 5, ErrorMessage = "Rating must be between 0 and 5")]
        public double Rating { get; set; }
        [Required]
        [RegularExpression(@"^(\+201|01)[0125][0-9]{8}$", ErrorMessage = "Invalid phone number.")]
        public List<string> PhoneNumbers { get; set; }
        [Required]
        public bool HaveDelivary { get; set; }
        [Required]
        [RegularExpression(@"^(\+201|01)[0125][0-9]{8}$", ErrorMessage = "Invalid phone number.")]
        public string PharmacistPhoneNumber { get; set; }

    }
}
