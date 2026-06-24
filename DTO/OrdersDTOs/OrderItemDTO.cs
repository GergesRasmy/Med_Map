namespace Med_Map.DTO.OrdersDTOs
{
    public class OrderItemDTO
    {
        [Required]
        public string type { get; set; } = "medicine"; // "medicine" | "service"
        public int quantity { get; set; }
        public Guid? medicineId { get; set; }
        public Guid? serviceId { get; set; }
    }
}
