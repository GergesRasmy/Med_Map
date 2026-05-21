namespace Med_Map.DTO.OrdersDTOs
{
    public class UpdateOrderDTO
    {
        [Required]
        public string nextStatus { get; set; }
        [Required]
        public string orderId { get; set; }
    }
}
