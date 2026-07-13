namespace Med_Map.Services
{
    public class EmailService : IEmailService
    {
        private readonly HttpClient _httpClient;
        private readonly IConfiguration _config;
        private readonly ILogger<EmailService> _logger;

        public EmailService(HttpClient httpClient, IConfiguration config, ILogger<EmailService> logger)
        {
            _httpClient = httpClient;
            _config = config;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string message)
        {
            _httpClient.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", _config["Mailtrap:ApiToken"]);

            var response = await _httpClient.PostAsJsonAsync("https://send.api.mailtrap.io/api/send", new
            {
                from = new { email = _config["Mailtrap:FromEmail"], name = _config["Mailtrap:FromName"] },
                to = new[] { new { email } },
                subject,
                html = message
            });

            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync();
                _logger.LogError("Mailtrap send failed {Status}: {Body}", response.StatusCode, body);
                throw new Exception("Failed to send email via Mailtrap");
            }
        }
    }
}
