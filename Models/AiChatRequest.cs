using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Med_Map.Models
{
    public enum AiChatMode
    {
        General = 0,
        Medical = 1,
    }
    public class AiChatRequest
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        [Required]
        public string QueryText { get; set; }
        [Required]
        public string ImageBase64 { get; set; }
        [Required]
        public int Mode { get; set; }
        [Required]
        public string ContextHistory { get; set; }
        [Required]
        public DateTime CreatedAt { get; set; }
        #region foreign keys
        [Required]
        public Guid SessionId { get; set; }
        [ForeignKey(nameof(SessionId))]
        public virtual AiChatSession Session { get; set; }
        #endregion

    }
}
