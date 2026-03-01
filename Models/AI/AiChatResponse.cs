using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Med_Map.Models.AI
{
    public enum DisclaimerType
    {
        Static,
        AIGenerated
    }
    public class AiChatResponse
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();
        [Required]
        public string ReplyText { get; set; }
        [Required]
        public double ConfidenceScore { get; set; }
        [Required]
        public DisclaimerType Disclaimer { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }

        #region foreign keys
        [Required]
        public Guid RequestId { get; set; }
        [ForeignKey(nameof(RequestId))]
        public virtual AiChatRequest Request { get; set; }
        #endregion
    }
}
