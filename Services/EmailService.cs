using MailKit.Net.Smtp;
using MailKit.Security;
using Med_Map.Services;
using MimeKit;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    public EmailService(IConfiguration config) => _config = config;

    public async Task SendEmailAsync(string email, string subject, string message)
    {
        var emailMessage = new MimeMessage();

        emailMessage.From.Add(new MailboxAddress("Med-Map", _config["Email:From"]));
        emailMessage.To.Add(new MailboxAddress("", email));
        emailMessage.Subject = subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = message };
        emailMessage.Body = bodyBuilder.ToMessageBody();

        using var client = new SmtpClient();
        try
        {
            // Use SecureSocketOptions.StartTls for Port 587
            await client.ConnectAsync(_config["Email:Host"],
                int.Parse(_config["Email:Port"]),
                SecureSocketOptions.StartTls);

            await client.AuthenticateAsync(_config["Email:Username"], _config["Email:Password"]);
            await client.SendAsync(emailMessage);
        }
        finally
        {
            await client.DisconnectAsync(true);
            client.Dispose();
        }
    }
}