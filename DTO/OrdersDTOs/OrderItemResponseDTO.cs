namespace Med_Map.DTO.OrdersDTOs
{
    public class OrderItemResponseDTO
    {
        public string type { get; set; } = "medicine"; // "medicine" | "service"

        // medicine fields (null for service items)
        public Guid? medicineId { get; set; }
        public string? medicineName { get; set; }

        // service fields (null for medicine items)
        public Guid? serviceId { get; set; }
        public string? serviceName { get; set; }

        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
