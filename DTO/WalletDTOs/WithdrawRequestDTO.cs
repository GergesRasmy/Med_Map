namespace Med_Map.DTO.WalletDTOs
{
    public class WithdrawRequestDTO
    {
        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "Amount must be greater than 0")]
        public decimal Amount { get; set; }

        [Required]
        public string Pin { get; set; } = string.Empty;

        [Required]
        public Dictionary<string, string> CashoutMethod { get; set; } = new();
    }
}
