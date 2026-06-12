using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Med_Map.Models.ordersANDmedicine
{
    [Index(nameof(TradeName), IsUnique = true)]
    [Index(nameof(RegistrationNo), IsUnique = true)]
    public class MedicineMaster
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        [MaxLength(300)]
        public string TradeName { get; set; }
        [Required]
        [MaxLength(300)]
        public string GenericName { get; set; }
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }
        [Required]
        public string ImageUrl { get; set; }
        [Required]
        public bool IsRestricted { get; set; }
        [Required]
        [MaxLength(300)]
        public string Manufacturer { get; set; }
        [MaxLength(100)]
        public string? DosageForm { get; set; }
        [MaxLength(100)]
        public string? Strength { get; set; }
        [MaxLength(50)]
        public string? Route { get; set; }
        [MaxLength(50)]
        public string? RegistrationNo { get; set; }
    }
}
