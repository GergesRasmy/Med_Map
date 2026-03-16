using System.ComponentModel.DataAnnotations.Schema;

namespace Med_Map.Models.Payments
{
    public enum PaymentStatus
    {
        Pending,
        Paid,
        Failed,
        Cancelled
    }
    public class Payment
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid OrderId { get; set; }
        public string UserId { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EGP";
        public string PaymentProvider { get; set; } = "Paymob";
        public string? ProviderOrderId { get; set; }
        public string? ProviderTransactionId { get; set; }
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public Orders Order { get; set; }
        public ICollection<PaymentLog> Logs { get; set; } = new List<PaymentLog>();
    }
}
