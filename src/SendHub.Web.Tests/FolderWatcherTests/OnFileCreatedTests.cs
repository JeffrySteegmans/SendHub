using System.Collections.Concurrent;
using System.IO.Abstractions.TestingHelpers;
using Microsoft.Extensions.Logging;
using Moq;
using SendHub.Web.Tests.Fakes;
using SendHub.Features.FileProcessing;

namespace SendHub.Web.Tests.FolderWatcherTests;

public sealed class OnFileCreatedTests
{
    private readonly Mock<ILogger<FolderWatcher>> loggerMock = new ();
    private readonly Mock<IApplicationSettings> settingsMock = new ();
    private readonly Mock<IFileScanner> fileScannerMock = new ();
    private readonly Mock<IFileSystemWatcherFactory> fileSystemWatcherFactoryMock = new ();
    private readonly FakeFileSystemWatcher fileSystemWatcher = new ();
    private readonly Mock<ICommandHandler<ProcessIncomingFile>> commandHandlerMock = new ();

    public OnFileCreatedTests()
    {
        fileSystemWatcherFactoryMock
            .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(fileSystemWatcher);

        settingsMock.Setup(x => x.WatchFolder).Returns(@"C:\Watch");
        settingsMock.Setup(x => x.DestinationFolder).Returns(@"C:\Destination");
        settingsMock.Setup(x => x.PollingIntervalSeconds).Returns(300);
    }

    [Fact]
    public async Task ShouldProcessCreatedFile()
    {
        var fileSystem = new MockFileSystem();

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
            settingsMock.Object,
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
