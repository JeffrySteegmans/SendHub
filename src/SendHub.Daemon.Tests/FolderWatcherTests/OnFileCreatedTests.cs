using System.Collections.Concurrent;
using System.IO.Abstractions.TestingHelpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SendHub.Daemon.Tests.Fakes;
using SendHub.Features.FileProcessing;

namespace SendHub.Daemon.Tests.FolderWatcherTests;

public sealed class OnFileCreatedTests
{
    private readonly Mock<ILogger<FolderWatcher>> loggerMock = new ();
    private readonly Mock<IFileScanner> fileScannerMock = new ();
    private readonly Mock<IFileSystemWatcherFactory> fileSystemWatcherFactoryMock = new ();
    private readonly FakeFileSystemWatcher fileSystemWatcher = new ();
    private readonly Mock<ICommandHandler<ProcessIncomingFile>> commandHandlerMock = new ();

    public OnFileCreatedTests()
    {
        fileSystemWatcherFactoryMock
            .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(fileSystemWatcher);
    }

    [Fact]
    public async Task ShouldProcessCreatedFile()
    {
        var fileSystem = new MockFileSystem();
        var settings = Options.Create(new FolderWatcherSettings
        {
            WatchFolder = @"C:\Watch",
            DestinationFolder = @"C:\Destination"
        });

        fileScannerMock
            .Setup(x => x.GetFiles(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);

        var processedFiles = new ConcurrentBag<string>();
        var fileProcessed = new TaskCompletionSource<bool>();
        commandHandlerMock
            .Setup(x => x.Handle(
                It.IsAny<ProcessIncomingFile>(),
                It.IsAny<CancellationToken>()))
            .Returns<ProcessIncomingFile, CancellationToken>(async (command, _) =>
            {
                processedFiles.Add(command.File.Name);
                fileProcessed.TrySetResult(true);
                await Task.CompletedTask;
            });

        var watcher = new FolderWatcher(
            loggerMock.Object,
            settings,
            fileSystem,
            fileScannerMock.Object,
            fileSystemWatcherFactoryMock.Object,
            commandHandlerMock.Object);

        var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        await watcher.StartAsync(
            cancellationTokenSource.Token);

        fileSystem.AddEmptyFile("test.txt");

        var args = new FileSystemEventArgs(WatcherChangeTypes.Created, ".", "test.txt");
        fileSystemWatcher
            .RaiseCreated(args);

        var timeout = Task.Delay(TimeSpan.FromSeconds(2));
        var winner = await Task.WhenAny(fileProcessed.Task, timeout);

        await watcher.StopAsync(
            cancellationTokenSource.Token);

        Assert.Same(fileProcessed.Task, winner);
        Assert.Single(processedFiles);
        Assert.Contains("test.txt", processedFiles);
    }
}
