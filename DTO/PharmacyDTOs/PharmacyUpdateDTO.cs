namespace Med_Map.DTO.PharmacyDTOs
{
    public class PharmacyUpdateDTO
    {
        public UpdateUserInfoDTO? userInfo { get; set; }

        [MinLength(3), MaxLength(30)]
        public string? pharmacyName { get; set; }
        public string? licenseNumber { get; set; }
        public string? address { get; set; }
        [Range(-90, 90)]
        public double? latitude { get; set; }
        [Range(-180, 180)]
        public double? longitude { get; set; }
        public TimeSpan? openingTime { get; set; }
        public TimeSpan? closingTime { get; set; }
        public bool? is24Hours { get; set; }
        public bool? deliveryAvailability { get; set; }
        public List<string>? pharmacyPhones { get; set; }
        public List<IFormFile>? nationalIds { get; set; }
        public List<IFormFile>? licenseImages { get; set; }
    }
}
