using System.Text.Json.Serialization;

namespace Med_Map.DTO.AiDTOs
{
    public class QueryAiDTO
    {
        [Required]
        [MinLength(1)]
        public string message { get; set; }

        public string lang { get; set; } = "auto";

        [JsonPropertyName("last_drug_id")]
        public string? lastDrugId { get; set; }
    }
}
