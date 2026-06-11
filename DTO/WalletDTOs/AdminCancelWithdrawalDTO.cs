namespace Med_Map.DTO.WalletDTOs
{
    public class AdminCancelWithdrawalDTO
    {
        [Required]
        public string Reason { get; set; } = string.Empty;
    }
}
