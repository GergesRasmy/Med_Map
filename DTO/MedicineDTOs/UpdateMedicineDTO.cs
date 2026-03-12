using System.ComponentModel.DataAnnotations.Schema;

namespace Med_Map.DTO.MedicineDTOs
{
    public class UpdateMedicineDTO
    {
        [Required]
        public string id { get; set; }
        [Required]
        [MinLength(3, ErrorMessage = "Minimum length is 3")]
        [MaxLength(30, ErrorMessage = "Maximum length is 30")]
        public string tradeName { get; set; }
        [Required]
        [MinLength(3, ErrorMessage = "Minimum length is 3")]
        [MaxLength(30, ErrorMessage = "Maximum length is 30")]
        public string genericName { get; set; }
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal price { get; set; }
        [Required]
        public IFormFile image { get; set; }
        [Required]
        public bool isRestricted { get; set; }
        [Required]
        [MinLength(3, ErrorMessage = "Minimum length is 3")]
        [MaxLength(30, ErrorMessage = "Maximum length is 30")]
        public string manufacturer { get; set; }
    }
}
