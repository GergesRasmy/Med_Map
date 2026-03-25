namespace Med_Map.DTO.PharmacyDTOs
{
    public class pharmacyProfileDTO
    {
        public string pharmacyName { get; set; }
        public List<string> pharmacyPhones { get; set; } = new();
        public string address { get; set; }
        public double latitude { get; set; }   
        public double longitude { get; set; }
        public TimeSpan openingTime { get; set; }
        public TimeSpan closingTime { get; set; }
        public bool is24Hours { get; set; }
        public bool deliveryAvailability { get; set; }
        public string licenseNumber { get; set; }
        public List<string> licenseImageUrls { get; set; } = new();
        public List<string> nationalIdUrls { get; set; } = new();
    }
}
