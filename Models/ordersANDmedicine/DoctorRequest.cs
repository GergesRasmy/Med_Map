using Med_Map.Models.customer;
using Med_Map.Models.pharmacy;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Med_Map.Models.ordersANDmedicine
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
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]

        public decimal ServicePrice { get; set; }
        [Required]
        [MinLength(3, ErrorMessage = "Minimum length is 3")]
        [MaxLength(30, ErrorMessage = "Maximum length is 30")]
        public string AssignedPersonnel { get; set; }

        #region foreign keys
        public Guid PharmacyProfileId { get; set; }
        [ForeignKey("PharmacyProfileId")]
        public PharmacyProfile Pharmacy { get; set; }

        [Required]
        public string CustomerApplicationUserId { get; set; }
        [ForeignKey("CustomerApplicationUserId")]
        public Customer Customer { get; set; }
        #endregion

    }
}
