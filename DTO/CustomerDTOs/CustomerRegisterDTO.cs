namespace Med_Map.DTO.CustomerDTOs
{
    public class CustomerRegisterDTO
    {
        public DateOnly? birthDate { get; set; }
        public string? medicalHistory { get; set; }
        public string? address { get; set; }
        public double? latitude { get; set; }
        public double? longitude { get; set; }
        public UpdateUserInfoDTO? userInfo { get; set; }
       
    }
}
