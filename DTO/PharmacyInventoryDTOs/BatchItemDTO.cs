namespace Med_Map.DTO.PharmacyInventoryDTOs
{
    public class BatchItemDTO
    {
        public Guid batchId { get; set; }
        public int quantity { get; set; }
        public DateOnly expiryDate { get; set; }
        public decimal price { get; set; }
    }
}
