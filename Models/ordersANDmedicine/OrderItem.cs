using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Med_Map.Models.ordersANDmedicine
{
    public class OrderItem
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        [Range(1,10,ErrorMessage ="Amount can't be over 10")]
        public int Quantity { get; set; }
     

        #region foreign keys
        [Required]
        public Guid OrderId { get; set; }
        [ForeignKey(nameof(OrderId))]
        public virtual Orders Order { get; set; }
        [Required]
        public Guid MedicineId { get; set; }
        [ForeignKey(nameof(MedicineId))]
        public virtual MedicineMaster Medicine { get; set; }
        #endregion
    }
}
