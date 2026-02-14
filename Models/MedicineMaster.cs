using System.ComponentModel.DataAnnotations;

namespace Med_Map.Models
{
    public class MedicineMaster
    {
        [Key]
        public Guid Id { get; set; } = new Guid();
        [Required]
        [MinLength(3, ErrorMessage = "Minimum length is 3")]
        [MaxLength(30, ErrorMessage = "Maximum length is 30")]
        public string TradeName { get; set; }
        [Required]
        [MinLength(3, ErrorMessage = "Minimum length is 3")]
        [MaxLength(30, ErrorMessage = "Maximum length is 30")]
        public string GenericName { get; set; }
        [Required]
        public string ImageUrl { get; set; }
        [Required]
        public bool IsRestricted { get; set; }
        [Required]
        [MinLength(3, ErrorMessage = "Minimum length is 3")]
        [MaxLength(30, ErrorMessage = "Maximum length is 30")]
        public string Manufacturer { get; set; }
    }
}
