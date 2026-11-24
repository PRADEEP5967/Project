using Microsoft.AspNetCore.Identity.UI.Services;
using System.Net;
using System.Net.Mail;

namespace WebApplication1.Services;

public interface IEmailService
{
    Task SendAsync(string recipient, string subject, string htmlBody, CancellationToken cancellationToken = default);
}

public sealed class SmtpEmailService : IEmailService, IEmailSender
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public Task SendEmailAsync(string email, string subject, string htmlMessage) =>
        SendAsync(email, subject, htmlMessage);

    public async Task SendAsync(string recipient, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var host = _configuration["Email:Host"]?.Trim();
        var from = _configuration["Email:FromAddress"]?.Trim();
        if (string.IsNullOrWhiteSpace(host) || string.IsNullOrWhiteSpace(from))
        {
            _logger.LogWarning("Email delivery is not configured; message for {Recipient} was not sent.", recipient);
            return;
        }

        using var message = new MailMessage(from, recipient, subject, htmlBody) { IsBodyHtml = true };
        using var client = new SmtpClient(host, _configuration.GetValue("Email:Port", 587))
        {
            EnableSsl = _configuration.GetValue("Email:UseStartTls", true),
            Credentials = new NetworkCredential(_configuration["Email:Username"], _configuration["Email:Password"])
        };

        await client.SendMailAsync(message, cancellationToken);
    }
}
