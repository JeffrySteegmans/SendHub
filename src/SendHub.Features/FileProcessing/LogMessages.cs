using Microsoft.Extensions.Logging;

namespace SendHub.Features.FileProcessing;

internal static partial class LogMessages
{
    [LoggerMessage(LogLevel.Information, "File {file} already processed, skipping")]
    public static partial void AlreadyProcessed(ILogger logger, string file);

    [LoggerMessage(LogLevel.Information, "Processing file {file} with {senderCount} senders")]
    public static partial void ProcessingFileWithSenders(ILogger logger, string file, int senderCount);

    [LoggerMessage(LogLevel.Information, "Sending {file} via {senderName}")]
    public static partial void SendingViaProvider(ILogger logger, string file, string senderName);

    [LoggerMessage(LogLevel.Information, "{file} sent via {senderName}")]
    public static partial void SentViaSender(ILogger logger, string file, string senderName);

    [LoggerMessage(LogLevel.Error, "Failed to send {file} via {senderName}")]
    public static partial void FailedToSend(ILogger logger, string file, string senderName, Exception exception);

    [LoggerMessage(LogLevel.Warning, "No file senders configured, skipping file {file}")]
    public static partial void NoSendersConfigured(ILogger logger, string file);

    [LoggerMessage(LogLevel.Information, "Successfully sent {file} via all senders")]
    public static partial void AllSendersSucceeded(ILogger logger, string file);

    [LoggerMessage(LogLevel.Error, "File processing failed: {failureCount} of {totalCount} senders failed for {file}")]
    public static partial void ProcessingFailed(ILogger logger, int failureCount, int totalCount, string file);

    [LoggerMessage(LogLevel.Information, "successfully processed {file}")]
    public static partial void FileProcessed(ILogger logger, string file);
}
