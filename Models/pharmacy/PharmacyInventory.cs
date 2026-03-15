using Med_Map.Models.ordersANDmedicine;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Med_Map.Models.pharmacy
{
    public class PharmacyInventory
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Price { get; set; }
        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Stock cannot be negative")]
        public int StockQuantity { get; set; }
        [Required]
        public DateOnly ExpiryDate { get; set; }
        [NotMapped]
        public bool IsAvailable => StockQuantity > 0 && ExpiryDate > DateOnly.FromDateTime(DateTime.UtcNow);
        #region foreign keys
        public Guid PharmacyProfileId { get; set; }
        [ForeignKey("PharmacyProfileId")]
        public PharmacyProfile Pharmacy { get; set; }
        [Required]
        public Guid MedicineId { get; set; }
        [ForeignKey(nameof(MedicineId))]
        public virtual MedicineMaster Medicine { get; set; }
        public Guid? LinkedAlternativeId { get; set; }
        [ForeignKey(nameof(LinkedAlternativeId))]
        public virtual PharmacyInventory? LinkedAlternative { get; set; }
        #endregion

    }
}
