namespace Med_Map.DTO.WalletDTOs
{
    public class WalletResponseDTO
    {
        public Guid Id { get; set; }
        public decimal CurrentBalance { get; set; }
        public decimal TotalEarnings { get; set; }
        public string Currency { get; set; } = string.Empty;
        public bool PinIsSet { get; set; }
        public DateTime? PinLockedUntil { get; set; }
    }
}
