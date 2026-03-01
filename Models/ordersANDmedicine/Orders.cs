using Med_Map.Models.customer;
using Med_Map.Models.pharmacy;
using NetTopologySuite.Geometries;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Med_Map.Models.ordersANDmedicine
{
    public enum StatusList
    {
        Pending,
        Preparing,
        Delivered,
        Cancelled
    }
    public enum PaymentOptions
    {
        Cash,
        Online
    }
    public class Orders
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]

        public DateTime CreatedAt { get; set; }
        [Required]
        public StatusList Status { get; set; }
        [Required]
        public PaymentOptions PaymentType { get; set; }
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal TotalAmount { get; set; }
        [Required]
        public Point DeliveryAddress { get; set; }
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
