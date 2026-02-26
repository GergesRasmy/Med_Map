namespace Med_Map.DTO
{
    public class VerifyOtpDTO
    {
        [Required]
        public Guid sessionId { get; set; } = Guid.NewGuid();
        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string code { get; set; }
    }
}
