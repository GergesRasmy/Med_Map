namespace Med_Map.DTO.AccountDTOs
{
    public class UpdateUserInfoDTO
    {
        [RegularExpression(@"^(\+201|01)[0125][0-9]{8}$", ErrorMessage = "Invalid phone number.")]
        public string? phoneNumber { get; set; }
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string? email { get; set; }
        public string? userName { get; set; }
        public string? currentPassword { get; set; }
        public string? newPassword { get; set; }
    }
}
