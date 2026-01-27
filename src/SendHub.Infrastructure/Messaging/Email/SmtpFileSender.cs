using Microsoft.Extensions.Logging;

namespace SendHub.Infrastructure.Messaging.Email;

internal sealed class SmtpFileSender(
    ILogger<SmtpFileSender> logger) : IFileSender
{
    public string Name => "Email (SMTP)";

    public Task Send(FileInfo file, CancellationToken ct)
    {
        logger.LogInformation("Sending {File} via SMTP", file.Name);

        return Task.CompletedTask;
    }
}
