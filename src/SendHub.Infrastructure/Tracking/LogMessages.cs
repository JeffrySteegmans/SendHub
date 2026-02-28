using Microsoft.Extensions.Logging;

namespace SendHub.Infrastructure.Tracking;

internal partial class LogMessages
{
    [LoggerMessage(LogLevel.Information, "No tracking file found at {Path}, starting fresh")]
    public static partial void NoTrackingFile(ILogger logger, string path);

    [LoggerMessage(LogLevel.Information, "Marked {FilePath} as processed")]
    public static partial void MarkedAsProcessed(ILogger logger, string filePath);

    [LoggerMessage(LogLevel.Information, "Loaded {Count} tracked files from {Path}")]
    public static partial void TrackedFilesLoaded(ILogger logger, int count, string path);

    [LoggerMessage(LogLevel.Warning, "Tracking file {Path} is corrupted, starting fresh")]
    public static partial void FileCorrupted(ILogger logger, string path, Exception ex);
}
