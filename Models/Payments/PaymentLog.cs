namespace Med_Map.Models.Payments
{
    public class PaymentLog
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid PaymentId { get; set; }
        public string Event { get; set; }
        public string Payload { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Payment Payment { get; set; }
    }
}
