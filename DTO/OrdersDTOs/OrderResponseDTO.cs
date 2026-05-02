namespace Med_Map.DTO.OrdersDTOs
{
    public class OrderResponseDTO
    {
        public Guid id { get; set; }
        public DateTime createdAt { get; set; }
        public decimal totalAmount { get; set; }
        public string status { get; set; }
        public List<OrderItemResponseDTO> items { get; set; }
        public string fulfillmentType { get; set; }
    }
}
