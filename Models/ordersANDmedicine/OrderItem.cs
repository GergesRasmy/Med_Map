using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Med_Map.Models.pharmacy;

namespace Med_Map.Models.ordersANDmedicine
{
    public class OrderItem
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        [Range(1,10,ErrorMessage ="Amount can't be over 10")]
        public int Quantity { get; set; }
        public decimal unitPrice { get; set; }

        #region foreign keys
        [Required]
        public Guid OrderId { get; set; }
        [ForeignKey(nameof(OrderId))]
        public virtual Orders Order { get; set; }

        public Guid? MedicineId { get; set; }
        [ForeignKey(nameof(MedicineId))]
        public virtual MedicineMaster? Medicine { get; set; }

        public Guid? ServiceId { get; set; }
        [ForeignKey(nameof(ServiceId))]
        public virtual PharmacyService? Service { get; set; }
        #endregion
    }
}
