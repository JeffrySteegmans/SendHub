using MailKit.Net.Smtp;
using Microsoft.Extensions.Options;
using MimeKit;

namespace SendHub.Features.Email.SendFile;

internal sealed class EmailSender(
    IOptions<EmailSenderConfig> options) : IFileSender
{
    public string Name
        => "Email";

    public async Task Send(
        FileInfo file,
        CancellationToken cancellationToken)
    {
        var emailConfig = options.Value;

        var mailMessage = new MimeMessage();
        mailMessage.From.Add(new MailboxAddress("Jeffry Steegmans", "jeffrysteegmans@gmail.com"));
        mailMessage.To.Add(new MailboxAddress(emailConfig.From, emailConfig.From));
        mailMessage.Subject = $"SendHub Notification – {file.Name}";

        var builder = new BodyBuilder
        {
            TextBody = $@"
Hello,

A new file has been processed by SendHub.

Details:
- File Name: {file.Name}
- Size: {file.Length} bytes
- Created: {file.CreationTime}
- Processed At: {DateTime.UtcNow} (UTC)

Regards,
SendHub Service"
        };
        await builder.Attachments
            .AddAsync(file.FullName, cancellationToken);

        mailMessage.Body = builder.ToMessageBody();

        using var client = new SmtpClient();

        await client
            .ConnectAsync(emailConfig.SmtpHost, emailConfig.SmtpPort, cancellationToken: cancellationToken);

        // Note: since we don't have an OAuth2 token, disable
        // the XOAUTH2 authentication mechanism.
        client.AuthenticationMechanisms
            .Remove("XOAUTH2");

        // Note: only needed if the SMTP server requires authentication
        await client
            .AuthenticateAsync(emailConfig.Username, emailConfig.Password, cancellationToken);
        await client
            .SendAsync(mailMessage, cancellationToken);
        await client
            .DisconnectAsync(true, cancellationToken);
    }
}
