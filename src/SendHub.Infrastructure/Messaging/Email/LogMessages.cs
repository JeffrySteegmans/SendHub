using Microsoft.Extensions.Logging;

namespace SendHub.Infrastructure.Messaging.Email;

internal partial class LogMessages
{
    [LoggerMessage(LogLevel.Information, "Sending '{FileName}' via SMTP to {To}")]
    public static partial void SendingFile(ILogger logger, string fileName, string to);

    [LoggerMessage(LogLevel.Information, "Successfully sent '{FileName}' via SMTP")]
    public static partial void FileSent(ILogger logger, string fileName);
}
