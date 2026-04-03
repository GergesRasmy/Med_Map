namespace Med_Map.DTO.PharmacyInventoryDTOs
{
    public class InventoryItemResponseDTO
    {
        public Guid medicineId { get; set; }
        public string? tradeName { get; set; }
        public string? genericName { get; set; }
        public int quantity { get; set; }
        public DateOnly expiryDate { get; set; }
        public decimal price { get; set; }
    }
}
