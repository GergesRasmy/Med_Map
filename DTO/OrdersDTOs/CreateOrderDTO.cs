namespace Med_Map.DTO.OrdersDTOs
{
    public class CreateOrderDTO
    {
        [Required]
        public string paymentOption { get; set; }
        [Required]
        public string fulfillmentType { get; set; }
        [Required]
        public double deliveryLongitude { get; set; }
        [Required]
        public double deliveryLatitude { get; set; }
        public string pharmacyId { get; set; }
        public List<OrderItemDTO> items { get; set; }
    }
}
