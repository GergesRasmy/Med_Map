namespace Med_Map.DTO.WalletDTOs
{
    public class SetPinDTO
    {
        [Required]
        [RegularExpression(@"^\d{4,6}$", ErrorMessage = "PIN must be 4 to 6 digits")]
        public string Pin { get; set; } = string.Empty;
    }
}
