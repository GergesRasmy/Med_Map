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
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        [Required]
        public StatusList Status { get; set; }
        [Required]
        public PaymentOptions PaymentType { get; set; }
        [Required]
        public Point DeliveryAddress { get; set; }
        [Required]
        [Column(TypeName = "decimal(18,2)")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Price must be greater than 0")]
        public decimal TotalAmount { get; set; }
        #region foreign keys
        public string CustomerId { get; set; } 

        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }

        public Guid PharmacyProfileId { get; set; }
        [ForeignKey("PharmacyProfileId")]
        public PharmacyProfile Pharmacy { get; set; }

        public virtual ICollection<OrderItem>? OrderItems { get; set; }
        #endregion


    }
}
