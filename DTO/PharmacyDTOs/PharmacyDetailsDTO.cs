namespace Med_Map.DTO.PharmacyDTOs
{
    public class PharmacyDetailsDTO
    {
        public string role { get; set; }
        public Guid id { get; set; }
        public string userName { get; set; }
        public string email { get; set; }
        public string? phoneNumber { get; set; }
        public string displayName { get; set; }
        public pharmacyProfileDTO? activeProfile { get; set; }   
        public pharmacyProfileDTO? pendingProfile { get; set; }
        public bool isRegistered { get; set; }= true;
    }
}
