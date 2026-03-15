using Med_Map.Models.pharmacy;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Med_Map.Models.ordersANDmedicine
{
    public class Wallet
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Balance can't be less than 0")]
        public decimal CurrentBalance { get; set; }
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0, double.MaxValue, ErrorMessage = "Earnings can't be less than 0")]
        public decimal TotalEarnings { get; set; }
        #region foreign keys
        public Guid PharmacyProfileId { get; set; }
        [ForeignKey("PharmacyProfileId")]
        public PharmacyProfile Pharmacy { get; set; }
        #endregion

    }
}
