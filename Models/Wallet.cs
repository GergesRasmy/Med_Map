using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Med_Map.Models
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
        [Required]
        public Guid PharmacyId { get; set; }
        [ForeignKey(nameof(PharmacyId))]
        public virtual Pharmacy Pharmacy { get; set; }
        #endregion

    }
}
