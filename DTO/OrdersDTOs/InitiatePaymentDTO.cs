namespace Med_Map.DTO.OrdersDTOs
{
    public class InitiatePaymentDTO
    {
        [Required]
        [MinLength(1, ErrorMessage = "At least one order ID is required")]
        public List<Guid> orderIds { get; set; }
    }
}
