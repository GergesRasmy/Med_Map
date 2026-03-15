namespace Med_Map.DTO.CustomerDTOs
{
    public class CustomerUpdateDTO
    {
        [Range(typeof(DateOnly), "01/01/1900", "01/01/2026", ErrorMessage = "Date must be in the format DD/MM/YYYY")]
        public DateOnly birthDate { get; set; }
        public string? medicalHistory { get; set; }
        public string? address { get; set; }

        [MinLength(3, ErrorMessage = "Minimum length is 3")]
        [MaxLength(30, ErrorMessage = "Maximum length is 30")]
        public string? userName { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? email { get; set; } 
        [RegularExpression(@"^(\+201|01)[0125][0-9]{8}$", ErrorMessage = "Invalid phone number.")]
        public string? phoneNumber { get; set; }
    }
}
