namespace Med_Map.DTO
{
    public class VerifyOtpDTO
    {
        [Required]
        public Guid SessionId { get; set; } = Guid.NewGuid();
        [Required]
        [StringLength(6, MinimumLength = 6)]
        public string Code { get; set; }
    }
}
