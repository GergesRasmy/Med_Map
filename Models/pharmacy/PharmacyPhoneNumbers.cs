using System.ComponentModel.DataAnnotations.Schema;

namespace Med_Map.Models.pharmacy
{
    public class PharmacyPhoneNumbers
    {
        [Key]
        public Guid Id { get; set; }= Guid.NewGuid();

        [Required]
        [RegularExpression(@"^(\+201|01)[0125][0-9]{8}$")]
        public string Number { get; set; }

        // Link back to Pharmacy
        public Guid PharmacyProfileId { get; set; }
        [ForeignKey("PharmacyProfileId")]
        public PharmacyProfile Pharmacy { get; set; }

    }
}
