namespace Med_Map.DTO.PharmacyInventoryDTOs
{
    public class PharmacyInvetoryDTO
    {
        [Required]
        public string pharmacyProfileId { get; set; }

        [Required]
        public Guid medicineId { get; set; }

        [Required]
        [Range(0, int.MaxValue, ErrorMessage = "Quantity must be a positive number.")]
        public int quantity { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateOnly expiryDate { get; set; }

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public decimal price { get; set; }
    }
}
