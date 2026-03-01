using System.ComponentModel.DataAnnotations.Schema;

namespace Med_Map.Models.pharmacy
{
    public enum DocumentType
    {
        NationalId,
        PharmacyLicense,
        Other
    }

    public class PharmacyDocument
    {
        [Key]
        public Guid Id { get; set; }= Guid.NewGuid();

        [Required]
        public string FileUrl { get; set; }

        [Required]
        public DocumentType Type { get; set; }

        // Link back to Pharmacy
        [Required]
        public Guid PharmacyId { get; set; }
        [ForeignKey("PharmacyId")]
        public Pharmacy Pharmacy { get; set; }
    }
}
