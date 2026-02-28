using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SendHub.Infrastructure.Messaging.Email;

internal sealed class SmtpFileSender(
    IOptions<SmtpSettings> options,
    ILogger<SmtpFileSender> logger) : SendHub.IFileSender
{
    private readonly SmtpSettings _settings = options.Value;

    public string Name => "Email (SMTP)";

    public async Task Send(FileInfo file, CancellationToken ct)
    {
        LogMessages.SendingFile(logger, file.Name, _settings.To);

        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl,
            Credentials = _settings.Username is not null
                ? new NetworkCredential(_settings.Username, _settings.Password)
                : null
        };

        using var attachment = new Attachment(file.FullName);
        using var message = new MailMessage(_settings.From, _settings.To)
        {
            Subject = $"SendHub: {file.Name}",
            Body = $"Please find the attached file: {file.Name}",
            Attachments = { attachment }
        };

        await client.SendMailAsync(message, ct);

        LogMessages.FileSent(logger, file.Name);
    }
}
