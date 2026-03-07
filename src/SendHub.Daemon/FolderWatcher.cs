using System.IO.Abstractions;
using System.Threading.Channels;
using SendHub.Features.FileProcessing;
using SendHub.ValueObjects;

namespace SendHub.Daemon;

internal sealed class FolderWatcher(
    ILogger<FolderWatcher> logger,
    IApplicationSettings settings,
    IFileSystem fileSystem,
    IFileScanner fileScanner,
    IFileSystemWatcherFactory watcherFactory,
    ICommandHandler<ProcessIncomingFile> processFileHandler) : BackgroundService
{
    private readonly Channel<FileInfo> _fileQueue = Channel.CreateUnbounded<FileInfo>();

    private IFileSystemWatcher? _fileSystemWatcher;

    private readonly DirectoryPath _watchFolder =
        DirectoryPath.From(settings.WatchFolder);

    private readonly DirectoryPath _destinationFolder =
        DirectoryPath.From(settings.DestinationFolder);

    public override Task StartAsync(
        CancellationToken cancellationToken)
    {
        if (!fileSystem.Directory.Exists(_destinationFolder))
        {
            fileSystem.Directory
                .CreateDirectory(_destinationFolder);

            FolderWatcherLogMessages
                .CreatedDestinationFolder(logger, _destinationFolder.Value);
        }

        _fileSystemWatcher = watcherFactory.Create(_watchFolder.Value,"*.*");
        _fileSystemWatcher.Created += OnFileCreated;
        _fileSystemWatcher.Error += OnWatcherError;

        return base.StartAsync(cancellationToken);
    }

    protected override async Task ExecuteAsync(
        CancellationToken cancellationToken)
    {
        await ScanExistingFiles(
            cancellationToken);

        _fileSystemWatcher!.EnableRaisingEvents = true;
        FolderWatcherLogMessages
            .WatchingFolder(logger, _watchFolder.Value);

        var workers = Enumerable.Range(0, 3)
            .Select(id => ProcessFileQueue(id, cancellationToken))
            .ToArray();

        await Task.WhenAll([..workers, PollForNewFiles(cancellationToken)]);
    }

    private async Task PollForNewFiles(CancellationToken cancellationToken)
    {
        while (true)
        {
            await Task.Delay(
                TimeSpan.FromSeconds(settings.PollingIntervalSeconds),
                cancellationToken);

            await ScanExistingFiles(cancellationToken);
        }
    }

    private async Task ProcessFileQueue(
        int workerId,
        CancellationToken cancellationToken)
    {
        FolderWatcherLogMessages
            .WorkerStarted(logger, workerId);

        await foreach (var file in _fileQueue.Reader.ReadAllAsync(cancellationToken))
        {
            try
            {
                await processFileHandler.Handle(
                    new ProcessIncomingFile(file, _destinationFolder.Value),
                    cancellationToken);
            }
            catch (Exception ex)
            {
                FolderWatcherLogMessages
                    .WorkerFailedProcessing(logger, workerId, file.FullName, ex);
            }
        }

        FolderWatcherLogMessages
            .WorkerStopped(logger, workerId);
    }

    private async Task ScanExistingFiles(
        CancellationToken cancellationToken)
    {
        FolderWatcherLogMessages
            .ScanningExistingFiles(logger, _watchFolder.Value);

        var files = await fileScanner.GetFiles(
            settings.WatchFolder,
            "*.*",
            cancellationToken);

        foreach (var file in files)
        {
            FolderWatcherLogMessages
                .FoundFile(logger, file.Name);
            await _fileQueue.Writer
                .WriteAsync(file, cancellationToken);
        }

        FolderWatcherLogMessages
            .FoundExistingFiles(logger, files.Count);
    }

    private void OnFileCreated(
        object sender,
        FileSystemEventArgs e)
    {
        try
        {
            var fileInfo = new FileInfo(e.FullPath);

            if (_fileQueue.Writer.TryWrite(fileInfo))
            {
                FolderWatcherLogMessages
                    .FileQueued(logger, fileInfo.Name);
            }
            else
            {
                FolderWatcherLogMessages
                    .QueueFull(logger, fileInfo.Name);
            }
        }
        catch (Exception ex)
        {
            FolderWatcherLogMessages
                .ErrorQueuingFile(logger, e.FullPath, ex);
        }
    }

    private void OnWatcherError(
        object sender,
        ErrorEventArgs e)
    {
        FolderWatcherLogMessages
            .FileSystemWatcherError(logger, e.GetException());

        TryRecoverFileSystemWatcher();
    }

    private void TryRecoverFileSystemWatcher()
    {
        if (_fileSystemWatcher is not { EnableRaisingEvents: true })
            return;

        _fileSystemWatcher.EnableRaisingEvents = false;
        _fileSystemWatcher.EnableRaisingEvents = true;
    }
}
