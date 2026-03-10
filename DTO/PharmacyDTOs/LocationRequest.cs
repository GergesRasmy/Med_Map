namespace Med_Map.DTO.PharmacyDTOs
{
    public class LocationRequest
    {
        [Required]
        [Range(-90, 90, ErrorMessage = "Latitude must be between -90 and 90.")]
        public double latitude { get; set; }

        [Required]
        [Range(-180, 180, ErrorMessage = "Longitude must be between -180 and 180.")]
        public double longitude { get; set; }

        [Range(0.1, 100, ErrorMessage = "Radius must be between 0.1 and 100 km.")]
        public double radiusInMeters { get; set; } = 5;
    }
}
