namespace Med_Map.DTO.WalletDTOs
{
    public class WalletTransactionDTO
    {
        public Guid Id { get; set; }
        public Guid WalletId { get; set; }
        public string PharmacyUserId { get; set; } = string.Empty;
        public string? PharmacyName { get; set; }
        public string Type { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? ResolvedAt { get; set; }
        public Guid? OrderId { get; set; }
        public Dictionary<string, string>? CashoutMethod { get; set; }
        public string? AdminNote { get; set; }
    }
}
