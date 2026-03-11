namespace Med_Map.DTO.PharmacyDTOs
{
    public class PharmacyDetailsDTO
    {
        public string role { get; set; }
        public Guid id { get; set; }
        public string email { get; set; }
        public string pharmacyName { get; set; }
        public List<string> pharmacyPhones { get; set; }
        public string doctorName { get; set; }
        public string doctorPhoneNumber { get; set; }
        public string address { get; set; }
        public Point cordinates { get; set; }
        public TimeSpan openingTime { get; set; }
        public TimeSpan closingTime { get; set; }
        public string licenseNumber { get; set; }
        public List<string> licenseImageUrls { get; set; }
        public List<string> nationalIdUrls { get; set; } 
        public bool is24Hours { get; set; }
        public bool delivaryAvailability { get; set; }
    }
}
