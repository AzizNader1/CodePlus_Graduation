using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;
using SkillSwap.Application.Common.Interfaces;

namespace SkillSwap.Infrastructure.Services;

public class GmailEmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<GmailEmailService> _logger;

    public GmailEmailService(IConfiguration configuration, ILogger<GmailEmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task SendEmailAsync(string toEmail, string subject, string htmlBody, CancellationToken cancellationToken = default)
    {
        var smtpHost = _configuration["Email:SmtpHost"] ?? "smtp.gmail.com";
        var smtpPort = int.TryParse(_configuration["Email:SmtpPort"], out var p) ? p : 587;
        var senderEmail = _configuration["Email:SenderEmail"] ?? "noreply.skillswap@gmail.com";
        var senderName = _configuration["Email:SenderName"] ?? "Skill Swap";
        var senderPassword = _configuration["Email:AppPassword"];

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(senderName, senderEmail));
        message.To.Add(new MailboxAddress(toEmail, toEmail));
        message.Subject = subject;

        var bodyBuilder = new BodyBuilder { HtmlBody = htmlBody };
        message.Body = bodyBuilder.ToMessageBody();

        if (string.IsNullOrWhiteSpace(senderPassword) || senderPassword.Contains("PLACEHOLDER"))
        {
            // Development / Test mode logging when App Password is not yet provided by user
            _logger.LogInformation("================ EMAIL DISPATCH (DEV MOCK) ================");
            _logger.LogInformation("To: {ToEmail}", toEmail);
            _logger.LogInformation("Subject: {Subject}", subject);
            _logger.LogInformation("Body: {Body}", htmlBody);
            _logger.LogInformation("===========================================================");
            return;
        }

        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.StartTls, cancellationToken);
            await client.AuthenticateAsync(senderEmail, senderPassword, cancellationToken);
            await client.SendAsync(message, cancellationToken);
            await client.DisconnectAsync(true, cancellationToken);

            _logger.LogInformation("Real email successfully sent via Gmail SMTP to {ToEmail}", toEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email via Gmail SMTP to {ToEmail}", toEmail);
        }
    }
}
