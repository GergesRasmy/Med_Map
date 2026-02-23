using Med_Map.Services;
using System.Net;
using System.Net.Mail;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    public EmailService(IConfiguration config) => _config = config;

    public async Task SendEmailAsync(string email, string subject, string message)
    {
        var client = new SmtpClient(_config["Email:Host"], int.Parse(_config["Email:Port"]))
        {
            Credentials = new NetworkCredential(_config["Email:Username"], _config["Email:Password"]),
            EnableSsl = true
        };

        var mailMessage = new MailMessage(_config["Email:From"], email, subject, message)
        {
            IsBodyHtml = true
        };

        await client.SendMailAsync(mailMessage);
    }
}