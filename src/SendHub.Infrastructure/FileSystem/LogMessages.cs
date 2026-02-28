using Microsoft.Extensions.Logging;

namespace SendHub.Infrastructure.FileSystem;

internal partial class LogMessages
{
    [LoggerMessage(LogLevel.Information, "Archived '{FileName}' to '{Destination}'")]
    public static partial void FileArchived(ILogger logger, string fileName, string destination);
}
