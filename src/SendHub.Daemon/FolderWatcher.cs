using SendHub.Features.Folder.MoveFile;
using SendHub.Features.Folder.Scan;

namespace SendHub.Daemon;

internal sealed class FolderWatcher(
    ILogger<FolderWatcher> logger,
    ICommandHandler<ScanFolder, ScanFolderResult> ScanFolder,
    IEnumerable<IFileSender> Senders,
    ICommandHandler<MoveFile> MoveFile) : BackgroundService
{
    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        const string FolderToWatch = @"C:\ScanFolder";
        var Destination = new DirectoryInfo(Path.Combine(FolderToWatch, "00_Processed"));
        var timer = new PeriodicTimer(TimeSpan.FromMinutes(1));

        do
        {
            try
            {
                if (!Directory.Exists(Destination.FullName))
                {
                    Directory.CreateDirectory(Destination.FullName);
                }

                LogMessages.ScanningFolder(logger, FolderToWatch);

                var result = await ScanFolder.Handle(new ScanFolder(FolderToWatch), stoppingToken);

                LogMessages.FolderScanned(logger, result.fileCount);
                foreach (var file in result.files)
                {
                    LogMessages.FileFound(logger, file.Name);
                    foreach (var sender in Senders)
                    {
                        await sender
                            .Send(file, stoppingToken);
                        LogMessages.FileSent(logger, file.Name, sender.Name);
                    }

                    await MoveFile.Handle(new MoveFile(
                        SourceFile: file,
                        Destination: Destination), stoppingToken);
                }
            }
            catch (Exception exception)
            {
                LogMessages.ExceptionWhileScanning(logger, exception.Message, exception);
            }
        } while (await timer.WaitForNextTickAsync(stoppingToken));
    }
}

internal static partial class LogMessages
{
    [LoggerMessage(LogLevel.Information, "Scanning folder '{folderPath}'...")]
    public static partial void ScanningFolder(ILogger logger, string folderPath);

    [LoggerMessage(LogLevel.Information, "Found {fileCount} file(s) in folder")]
    public static partial void FolderScanned(ILogger logger, int fileCount);

    [LoggerMessage(LogLevel.Debug, "\t- {fileName}")]
    public static partial void FileFound(ILogger logger, string fileName);

    [LoggerMessage(LogLevel.Information, "File '{fileName}' sent using '{senderName}' sender")]
    public static partial void FileSent(ILogger logger, string fileName, string senderName);

    [LoggerMessage(LogLevel.Error, "Exception occurred while scanning folder: {exceptionMessage}")]
    public static partial void ExceptionWhileScanning(ILogger logger, string exceptionMessage, Exception exception);
}
