namespace Med_Map.DTO.PharmacyInventoryDTOs
{
    public class UpdateInventoryDTO
    {
        [Required]
        public Guid medicineId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a positive number.")]
        public int? quantity { get; set; }

        [DataType(DataType.Date)]
        public DateOnly? expiryDate { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public decimal? price { get; set; }
    }
}
