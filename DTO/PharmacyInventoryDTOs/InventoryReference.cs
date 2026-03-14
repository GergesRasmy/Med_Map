namespace Med_Map.DTO.PharmacyInventoryDTOs
{
    public class InventoryReference
    {
        [Required]
        public string PharmacyId { get; set; }

        [Required]
        public Guid MedicineId { get; set; }
    }
}
