namespace Med_Map.DTO.CustomerDTOs
{
    public class CustomerRegisterDTO
    {
        public DateOnly? birthDate { get; set; }
        public string? medicalHistory { get; set; }
        public string? address { get; set; }
        public UpdateUserInfoDTO? userInfo { get; set; }
       
    }
}
