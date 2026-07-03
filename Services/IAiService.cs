namespace Med_Map.Services
{
    public class AiRelayResponse
    {
        public int StatusCode { get; set; }
        public string Content { get; set; } = string.Empty;
    }

    public interface IAiService
    {
        Task<AiRelayResponse> QueryAsync(QueryAiDTO model);
        Task<AiRelayResponse> OcrMedicineAsync(IFormFile file);
        Task<AiRelayResponse> OcrPrescriptionAsync(IFormFile file);
    }
}
