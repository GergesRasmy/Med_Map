namespace Med_Map.DTO.PharmacyServiceDTOs
{
    public class UpdatePharmacyServiceDTO
    {
        [Required]
        public Guid Id { get; set; }

        [MaxLength(200)]
        public string? Name { get; set; }

        [MaxLength(1000)]
        public string? Description { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal? Price { get; set; }

        public bool? IsActive { get; set; }
    }
}
