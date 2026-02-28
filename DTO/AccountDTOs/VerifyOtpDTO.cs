namespace Med_Map.DTO.AccountDTOs
{
    public class VerifyOtpDTO
    {
        [Required]
        public Guid sessionId { get; set; } = Guid.NewGuid();
        [Required]
        [StringLength(6, MinimumLength = 6)]
        public required string code { get; set; }
    }
}
