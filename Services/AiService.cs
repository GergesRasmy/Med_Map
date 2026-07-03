using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace Med_Map.Services
{
    public class AiService : IAiService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<AiService> _logger;

        public AiService(HttpClient httpClient, ILogger<AiService> logger)
        {
            _httpClient = httpClient;
            _logger = logger;
        }

        public Task<AiRelayResponse> QueryAsync(QueryAiDTO model)
            => SendAsync(() => _httpClient.PostAsJsonAsync("query", model));

        public Task<AiRelayResponse> OcrMedicineAsync(IFormFile file)
            => SendFileAsync("ocr/medicine", file);

        public Task<AiRelayResponse> OcrPrescriptionAsync(IFormFile file)
            => SendFileAsync("ocr/prescription", file);

        private Task<AiRelayResponse> SendFileAsync(string path, IFormFile file)
        {
            return SendAsync(() =>
            {
                var content = new MultipartFormDataContent();
                var streamContent = new StreamContent(file.OpenReadStream());
                streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType ?? "application/octet-stream");
                content.Add(streamContent, "file", file.FileName);
                return _httpClient.PostAsync(path, content);
            });
        }

        private async Task<AiRelayResponse> SendAsync(Func<Task<HttpResponseMessage>> send)
        {
            try
            {
                using var response = await send();
                var body = await response.Content.ReadAsStringAsync();
                return new AiRelayResponse { StatusCode = (int)response.StatusCode, Content = body };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AI service call failed");
                return new AiRelayResponse
                {
                    StatusCode = StatusCodes.Status502BadGateway,
                    Content = "{\"success\":false,\"code\":\"502\",\"message\":\"AI service unavailable\",\"error\":\"AI service unavailable\"}"
                };
            }
        }
    }
}
