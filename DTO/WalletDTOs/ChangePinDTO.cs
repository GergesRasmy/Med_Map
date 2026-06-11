namespace Med_Map.DTO.WalletDTOs
{
    public class ChangePinDTO
    {
        [Required]
        public string CurrentPin { get; set; } = string.Empty;

        [Required]
        [RegularExpression(@"^\d{4,6}$", ErrorMessage = "New PIN must be 4 to 6 digits")]
        public string NewPin { get; set; } = string.Empty;
    }
}
