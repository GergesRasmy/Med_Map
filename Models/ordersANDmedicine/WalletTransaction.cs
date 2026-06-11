using Med_Map.Models.pharmacy;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Med_Map.Models.ordersANDmedicine
{
    public class WalletTransaction
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public TransactionType Type { get; set; }

        [Required]
        public TransactionStatus Status { get; set; } = TransactionStatus.Completed;

        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue)]
        public decimal Amount { get; set; }

        [Required]
        public CurrencyType Currency { get; set; }

        [Required]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? ResolvedAt { get; set; }

        public Guid? OrderId { get; set; }
        [ForeignKey("OrderId")]
        public Orders? Order { get; set; }

        public string? CashoutMethodJson { get; set; }

        public string? AdminNote { get; set; }

        [Required]
        public Guid WalletId { get; set; }
        [ForeignKey("WalletId")]
        public Wallet Wallet { get; set; } = null!;
    }
}
