using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using MailKit.Security;

namespace Remp.API.Services.Email;

public class EmailSender : IEmailSender
{
    private readonly EmailSettings _emailSettings;

    public EmailSender(IOptions<EmailSettings> emailSettings)
    {
        _emailSettings = emailSettings.Value;
    }
    public async Task SendEmailAsync(string title, string body, string receiver)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(body);
        ArgumentException.ThrowIfNullOrWhiteSpace(receiver);

        using var message = new MimeMessage();

        var sender = new MailboxAddress(
            _emailSettings.SenderName,
            _emailSettings.SenderEmail);

        message.From.Add(sender);

        var recipient = MailboxAddress.Parse(receiver);

        message.To.Add(recipient);
        message.Subject = title;
        var textPart = new TextPart("plain");
        textPart.Text = body;
        message.Body = textPart;

        using SmtpClient client = new SmtpClient();

        await client.ConnectAsync(_emailSettings.Host, _emailSettings.Port, SecureSocketOptions.StartTls);
        await client.AuthenticateAsync(_emailSettings.Username, _emailSettings.Password);
        await client.SendAsync(message);
        await client.DisconnectAsync(true);

    }
}