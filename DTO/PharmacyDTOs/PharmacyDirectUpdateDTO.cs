namespace Med_Map.DTO.PharmacyDTOs
{
    public class PharmacyDirectUpdateDTO
    {
        // Sensitive — changes go to pending profile for admin review
        [MinLength(3), MaxLength(100)]
        public string? pharmacyName { get; set; }
        public string? address { get; set; }
        [Range(-90, 90)]
        public double? latitude { get; set; }
        [Range(-180, 180)]
        public double? longitude { get; set; }

        // Safe — applied instantly to the active profile
        public List<string>? pharmacyPhones { get; set; }
        public TimeSpan? openingTime { get; set; }
        public TimeSpan? closingTime { get; set; }
        public bool? is24Hours { get; set; }
        public bool? deliveryAvailability { get; set; }
    }
}
