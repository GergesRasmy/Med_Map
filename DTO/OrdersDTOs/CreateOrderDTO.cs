namespace Med_Map.DTO.OrdersDTOs
{
    public class CreateOrderDTO
    {
        [Required]
        public string paymentOption { get; set; }
        [Required]
        public string fulfillmentType { get; set; }
        [Required]
        public string phoneNumber { get; set; }
        public double deliveryLongitude { get; set; }
        public double deliveryLatitude { get; set; }
        public string? deliveryAddress { get; set; }
        public string pharmacyId { get; set; }
        public List<OrderItemDTO> items { get; set; }
    }
}
