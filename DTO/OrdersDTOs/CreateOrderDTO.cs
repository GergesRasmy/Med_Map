namespace Med_Map.DTO.OrdersDTOs
{
    public class CreateOrderDTO
    {
        [Required]
        public string paymentOption { get; set; }
        [Required]
        public double longitude { get; set; }
        [Required]
        public double latitude { get; set; }
        public Guid pharmacyId { get; set; }
        public List<OrderItemDTO> items { get; set; }
    }
}
