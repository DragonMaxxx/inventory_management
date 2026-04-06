using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using Trisecmed.Application.Notifications;

namespace Trisecmed.Infrastructure.Email;

public class SmtpEmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration config, ILogger<SmtpEmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        var smtpSection = _config.GetSection("Smtp");

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(
            smtpSection["SenderName"] ?? "Trisecmed",
            smtpSection["SenderEmail"] ?? "noreply@trisecmed.local"));
        message.To.Add(MailboxAddress.Parse(to));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(
                smtpSection["Host"] ?? "localhost",
                int.Parse(smtpSection["Port"] ?? "587"),
                MailKit.Security.SecureSocketOptions.StartTlsWhenAvailable, ct);

            var user = smtpSection["Username"];
            if (!string.IsNullOrEmpty(user))
                await client.AuthenticateAsync(user, smtpSection["Password"], ct);

            await client.SendAsync(message, ct);
            await client.DisconnectAsync(true, ct);

            _logger.LogInformation("Email sent to {To}: {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}: {Subject}", to, subject);
            throw;
        }
    }
}
