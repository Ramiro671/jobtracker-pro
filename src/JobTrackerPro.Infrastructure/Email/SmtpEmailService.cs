using System.Net;
using System.Net.Mail;
using JobTrackerPro.Application.Common.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace JobTrackerPro.Infrastructure.Email;

/// <summary>
/// Sends emails via SMTP (Gmail, SendGrid relay, etc.).
/// Set EmailSettings:Enabled = true and provide credentials to activate.
/// When disabled, the email content is logged at Information level.
/// </summary>
public class SmtpEmailService : IEmailService
{
    private readonly EmailSettings _settings;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<EmailSettings> settings, ILogger<SmtpEmailService> logger)
    {
        _settings = settings.Value;
        _logger   = logger;
    }

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        if (!_settings.Enabled)
        {
            _logger.LogInformation(
                "[Email disabled] To: {To} | Subject: {Subject}", to, subject);
            return;
        }

        using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
        {
            EnableSsl   = true,
            Credentials = new NetworkCredential(_settings.Username, _settings.Password)
        };

        using var message = new MailMessage
        {
            From       = new MailAddress(_settings.FromEmail, _settings.FromName),
            Subject    = subject,
            Body       = htmlBody,
            IsBodyHtml = true
        };
        message.To.Add(to);

        await client.SendMailAsync(message, cancellationToken);
        _logger.LogInformation("Email sent to {To}: {Subject}", to, subject);
    }
}
