namespace Med_Map.DTO.PharmacyDTOs
{
    public class RejectProfileDTO
    {
        [Required, MinLength(3), MaxLength(500)]
        public string reason { get; set; }
    }
}
