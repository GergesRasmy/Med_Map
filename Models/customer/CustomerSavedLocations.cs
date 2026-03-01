using System.ComponentModel.DataAnnotations.Schema;

namespace Med_Map.Models.customer
{
    public class CustomerSavedLocations
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public Point Location { get; set; }

        public string? LocationName { get; set; } 

        // Foreign Key to Customer
        [Required]
        public Guid CustomerId { get; set; }
        [ForeignKey("CustomerId")]
        public Customer Customer { get; set; }
    }
}
