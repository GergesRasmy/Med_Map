using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Med_Map.Models
{
    public enum RequestStatus
    {
        Pending,
        Approved,
        Rejected
    }
    public class WithdrawalRequest
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Withdrawal must be greater than 0")]
        public decimal Amount { get; set; }
        [Required]
        public DateTime RequestDate { get; set; }
        [Required]
        public RequestStatus Status { get; set; }
        [Required]
        public string AdminComment { get; set; }
        [Required]
        public string ReceiptImage { get; set; }
        #region foreign keys
        public Guid PharmacyId { get; set; }
        [ForeignKey(nameof(PharmacyId))]
        public virtual Pharmacy Pharmacy { get; set; }
        #endregion
    }
}
