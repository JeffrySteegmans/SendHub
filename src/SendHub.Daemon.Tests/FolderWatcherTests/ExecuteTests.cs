using System.Collections.Concurrent;
using System.IO.Abstractions.TestingHelpers;
using Microsoft.Extensions.Logging;
using Moq;
using SendHub.Features.FileProcessing;

namespace SendHub.Daemon.Tests.FolderWatcherTests;

public sealed class ExecuteTests
{
    private readonly Mock<ILogger<FolderWatcher>> loggerMock = new ();
    private readonly Mock<IApplicationSettings> settingsMock = new ();
    private readonly Mock<IFileScanner> fileScannerMock = new ();
    private readonly Mock<IFileSystemWatcherFactory> fileSystemWatcherFactoryMock = new ();
    private readonly Mock<IFileSystemWatcher> fileSystemWatcherMock = new ();
    private readonly Mock<ICommandHandler<ProcessIncomingFile>> commandHandlerMock = new ();

    public ExecuteTests()
    {
        fileSystemWatcherFactoryMock
            .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(fileSystemWatcherMock.Object);

        settingsMock.Setup(x => x.WatchFolder).Returns(@"C:\Watch");
        settingsMock.Setup(x => x.DestinationFolder).Returns(@"C:\Destination");
        settingsMock.Setup(x => x.PollingIntervalSeconds).Returns(300);
    }

    [Fact]
    public async Task ShouldProcessAllFiles()
    {
        var existingFiles = new List<FileInfo>
        {
            new (@"C:\Watch\file1.txt"),
            new (@"C:\Watch\file2.txt"),
            new (@"C:\Watch\file3.txt")
        };
        var scanCompleted = new TaskCompletionSource<bool>();
        fileScannerMock
            .Setup(x => x.GetFiles(
                It.IsAny<string>(),
                It.IsAny<string>(),
                It.IsAny<CancellationToken>()))
            .Callback((string _, string _, CancellationToken _) => scanCompleted.TrySetResult(true))
            .ReturnsAsync(existingFiles);

        var processedFiles = new ConcurrentBag<string>();
        var allProcessed = new TaskCompletionSource<bool>();
        commandHandlerMock
            .Setup(x => x.Handle(
                It.IsAny<ProcessIncomingFile>(),
                It.IsAny<CancellationToken>()))
            .Returns<ProcessIncomingFile, CancellationToken>(async (command, _) =>
            {
                processedFiles.Add(command.File.Name);
                if (processedFiles.Count == existingFiles.Count)
                {
                    allProcessed.TrySetResult(true);
                }
                await Task.CompletedTask;
            });

        var watcher = new FolderWatcher(
            loggerMock.Object,
            settingsMock.Object,
            new MockFileSystem(),
            fileScannerMock.Object,
            fileSystemWatcherFactoryMock.Object,
            commandHandlerMock.Object);

        var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(2));

        await watcher.StartAsync(
            cancellationTokenSource.Token);

        await scanCompleted.Task;
        // Wait for all files to be processed
        var timeout = Task.Delay(TimeSpan.FromSeconds(2));
        var winner = await Task.WhenAny(allProcessed.Task, timeout);

        await watcher.StopAsync(
            cancellationTokenSource.Token);

        Assert.Same(allProcessed.Task, winner);
        Assert.Equal(3, processedFiles.Count);
        Assert.Contains("file1.txt", processedFiles);
        Assert.Contains("file2.txt", processedFiles);
        Assert.Contains("file3.txt", processedFiles);
    }
}
