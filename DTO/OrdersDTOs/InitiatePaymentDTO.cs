namespace Med_Map.DTO.OrdersDTOs
{
    public class InitiatePaymentDTO
    {
        [Required]
        public Guid orderId { get; set; }
    }
}
