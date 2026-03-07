namespace SendHub.Web;

internal static partial class FolderWatcherLogMessages
{
    [LoggerMessage(LogLevel.Information, "Created destination folder {folder}")]
    public static partial void CreatedDestinationFolder(ILogger logger, string folder);

    [LoggerMessage(LogLevel.Information, "Scanning {folder} for existing files")]
    public static partial void ScanningExistingFiles(ILogger logger, string folder);

    [LoggerMessage(LogLevel.Information, "Found {count} existing files")]
    public static partial void FoundExistingFiles(ILogger logger, int count);

    [LoggerMessage(LogLevel.Debug, "Found existing file: {fileName}")]
    public static partial void FoundFile(ILogger logger, string fileName);

    [LoggerMessage(LogLevel.Information, "Watching folder {folder} for new files")]
    public static partial void WatchingFolder(ILogger logger, string folder);

    [LoggerMessage(LogLevel.Information, "Queued file {fileName}")]
    public static partial void FileQueued(ILogger logger, string fileName);

    [LoggerMessage(LogLevel.Warning, "Queue full, file dropped: {fileName}")]
    public static partial void QueueFull(ILogger logger, string fileName);

    [LoggerMessage(LogLevel.Error, "Error queuing file {path}")]
    public static partial void ErrorQueuingFile(ILogger logger, string path, Exception exception);

    [LoggerMessage(LogLevel.Information, "Worker {workerId} started")]
    public static partial void WorkerStarted(ILogger logger, int workerId);

    [LoggerMessage(LogLevel.Error, "Worker {workerId} failed processing {file}")]
    public static partial void WorkerFailedProcessing(ILogger logger, int workerId, string file, Exception exception);

    [LoggerMessage(LogLevel.Information, "Worker {workerId} stopped")]
    public static partial void WorkerStopped(ILogger logger, int WorkerId);

    [LoggerMessage(LogLevel.Error, "FileSystemWatcher error")]
    public static partial void FileSystemWatcherError(ILogger logger, Exception exception);
}
