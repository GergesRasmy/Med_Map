using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Med_Map.Models
{
    public enum Services
    {
        Injection,
        FirstAid,
        Consultation
    }
    public enum ServiceStatus
    {
        Pending,
        Approved,
        Paid
    }
    public class DoctorRequest
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public Services ServiceType { get; set; }
        [Required]
        public ServiceStatus Status { get; set; }
        [Required]
        [MinLength(3, ErrorMessage = "Minimum length is 3")]
        [MaxLength(30, ErrorMessage = "Maximum length is 30")]
        public string AssignedPersonnel { get; set; }

        #region foreign keys
        [Required]
        public Guid PharmacyId { get; set; }
        [ForeignKey(nameof(PharmacyId))]
        public virtual Pharmacy Pharmacy { get; set; }
        [Required]
        public Guid CustomerId { get; set; }
        [ForeignKey(nameof(CustomerId))]
        public virtual Customer Customer { get; set; }
        #endregion

    }
}
