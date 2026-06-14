namespace Med_Map.DTO.PharmacyInventoryDTOs
{
    public class InventorySearchResultDTO
    {
        public string pharmacyId { get; set; }
        public string pharmacyName { get; set; }
        public Guid medicineId { get; set; }
        public string tradeName { get; set; }
        public string genericName { get; set; }
        public decimal price { get; set; }
        public int stock { get; set; }
        public DateOnly expiryDate { get; set; }
    }
}
