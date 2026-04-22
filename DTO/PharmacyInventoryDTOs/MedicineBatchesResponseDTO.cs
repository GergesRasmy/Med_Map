namespace Med_Map.DTO.PharmacyInventoryDTOs
{
    public class MedicineBatchesResponseDTO
    {
        public Guid medicineId { get; set; }
        public string? tradeName { get; set; }
        public string? genericName { get; set; }
        public int totalStock { get; set; }
        public List<BatchItemDTO> batches { get; set; } = new();
    }
}
