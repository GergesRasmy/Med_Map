namespace Med_Map.DTO.PharmacyDTOs
{
    public class RegisterPharmacyDTO
    {
        public UpdateUserInfoDTO? userInfo { get; set; }

        [Required]
        [MinLength(3), MaxLength(30)]
        public string pharmacyName { get; set; }
        [Required]
        public string licenseNumber { get; set; }
        [Required]
        public string address { get; set; }
        [Required]
        [Range(-90, 90)]
        public double latitude { get; set; }
        [Required]
        [Range(-180, 180)]
        public double longitude { get; set; }
        [Required]
        public TimeSpan openingTime { get; set; }
        [Required]
        public TimeSpan closingTime { get; set; }
        [Required]
        public bool is24Hours { get; set; }
        [Required]
        public bool deliveryAvailability { get; set; }
        [Required]
        public List<string> pharmacyPhones { get; set; } = new();
        [Required]
        public List<IFormFile> nationalIds { get; set; } = new();
        [Required]
        public List<IFormFile> licenseImages { get; set; } = new();
    }

}
