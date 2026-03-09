namespace Med_Map.DTO.CustomerDTOs
{
    public class CustomerDetailsDTO
    {
        public DateOnly birthDate { get; set; }
        public string medicalHistory { get; set; }
        public string userName { get; set; }
        public string email { get; set; }
        public string phoneNumber { get; set; }
    }
}
