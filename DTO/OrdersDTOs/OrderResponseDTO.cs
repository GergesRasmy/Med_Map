namespace Med_Map.DTO.OrdersDTOs
{
    public class OrderResponseDTO
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; }
        public List<OrderItemResponseDTO> Items { get; set; }
    }
}
