using System.ComponentModel.DataAnnotations.Schema;

namespace Med_Map.DTO.MedicineDTOs
{
    public class AddMedicineDTO
    {
        [Required]
        [MaxLength(300)]
        public string tradeName { get; set; }
        [Required]
        [MaxLength(300)]
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
        [MaxLength(300)]
        public string manufacturer { get; set; }
        [MaxLength(100)]
        public string? dosageForm { get; set; }
        [MaxLength(100)]
        public string? strength { get; set; }
        [MaxLength(50)]
        public string? route { get; set; }
        [MaxLength(50)]
        public string? registrationNo { get; set; }
    }
}
