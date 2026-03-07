using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;

namespace SendHub.Infrastructure.Messaging.Email;

internal sealed class SmtpFileSender(
    IApplicationSettings settings,
    ILogger<SmtpFileSender> logger) : SendHub.IFileSender
{
    public string Name => "Email (SMTP)";

    public async Task Send(FileInfo file, CancellationToken ct)
    {
        LogMessages.SendingFile(logger, file.Name, settings.SmtpTo);

        using var client = new SmtpClient(settings.SmtpHost, settings.SmtpPort)
        {
            EnableSsl = settings.SmtpEnableSsl,
            Credentials = settings.SmtpUsername is not null
                ? new NetworkCredential(settings.SmtpUsername, settings.SmtpPassword)
                : null
        };

        using var attachment = new Attachment(file.FullName);
        using var message = new MailMessage(settings.SmtpFrom, settings.SmtpTo)
        {
            Subject = $"SendHub: {file.Name}",
            Body = $"Please find the attached file: {file.Name}",
            Attachments = { attachment }
        };

        await client.SendMailAsync(message, ct);

        LogMessages.FileSent(logger, file.Name);
    }
}
