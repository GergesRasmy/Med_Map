namespace Med_Map.Models.Payments
{
    public class PaymentOrder
    {
        public Guid PaymentId { get; set; }
        public Payment Payment { get; set; }
        public Guid OrderId { get; set; }
        public Orders Order { get; set; }
    }
}
