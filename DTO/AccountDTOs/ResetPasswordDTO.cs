namespace Med_Map.DTO.AccountDTOs
{
    public class ResetPasswordDTO
    {
        [Required]
        public Guid sessionId { get; set; }

        [Required]
        [StringLength(6, MinimumLength = 6)]
        public required string code { get; set; }

        [Required]
        [MinLength(8)]
        public required string newPassword { get; set; }
    }
}
