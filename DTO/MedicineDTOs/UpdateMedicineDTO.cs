using System.ComponentModel.DataAnnotations.Schema;

namespace Med_Map.DTO.MedicineDTOs
{
    public class UpdateMedicineDTO
    {
        [Required]
        public string id { get; set; }
        [MaxLength(300)]
        public string? tradeName { get; set; }
        [MaxLength(300)]
        public string? genericName { get; set; }
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal? price { get; set; }
        public IFormFile? image { get; set; }
        public bool? isRestricted { get; set; }
        [MaxLength(300)]
        public string? manufacturer { get; set; }
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
