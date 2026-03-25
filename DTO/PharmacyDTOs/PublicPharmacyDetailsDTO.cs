namespace Med_Map.DTO.PharmacyDTOs
{
    public class PublicPharmacyDetailsDTO
    {
        public string role { get; set; }
        public Guid id { get; set; }
        public string pharmacyName { get; set; }
        public List<string> pharmacyPhones { get; set; }
        public string userName { get; set; }
        public string displayName { get; set; }

        public string address { get; set; }
        public double latitude { get; set; } 
        public double longitude { get; set; }
        public TimeSpan openingTime { get; set; }
        public TimeSpan closingTime { get; set; }
        public bool is24Hours { get; set; }
        public bool deliveryAvailability { get; set; }
        
    }
}
