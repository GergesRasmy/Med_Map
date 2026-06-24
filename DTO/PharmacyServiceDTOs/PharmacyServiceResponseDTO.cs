namespace Med_Map.DTO.PharmacyServiceDTOs
{
    public class PharmacyServiceResponseDTO
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public bool IsActive { get; set; }
        public string PharmacyUserId { get; set; } = string.Empty;
        public string? PharmacyName { get; set; }
    }
}
