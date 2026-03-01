using Med_Map.Models.AI;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Med_Map.Models.ordersANDmedicine
{
    public class Recommendation
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public string MedicineName { get; set; }
        [Required]
        public string Reason { get; set; }
        [Required]
        public string DosageInfo { get; set; }
        [Required]
        public string SearchQuery { get; set; }

        #region foreign keys
        [Required]
        public Guid ResponseId { get; set; }
        [ForeignKey(nameof(ResponseId))]
        public virtual AiChatResponse Response { get; set; }
        #endregion
    
    }
}
