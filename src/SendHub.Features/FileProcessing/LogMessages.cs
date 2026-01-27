using Microsoft.Extensions.Logging;

namespace SendHub.Features.FileProcessing;

internal static partial class LogMessages
{
    [LoggerMessage(LogLevel.Information, "Processing file {file}")]
    public static partial void ProcessingFile(ILogger logger, string file);
}
