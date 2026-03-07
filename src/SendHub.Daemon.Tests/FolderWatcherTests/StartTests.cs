using System.IO.Abstractions.TestingHelpers;
using Microsoft.Extensions.Logging;
using Moq;
using SendHub.Features.FileProcessing;

namespace SendHub.Daemon.Tests.FolderWatcherTests;

public sealed class StartTests
{
    private readonly Mock<ILogger<FolderWatcher>> loggerMock = new ();
    private readonly Mock<IApplicationSettings> settingsMock = new ();
    private readonly Mock<IFileScanner> fileScannerMock = new ();
    private readonly Mock<IFileSystemWatcherFactory> fileSystemWatcherFactoryMock = new ();
    private readonly Mock<IFileSystemWatcher> fileSystemWatcherMock = new ();
    private readonly Mock<ICommandHandler<ProcessIncomingFile>> commandHandlerMock = new ();

    public StartTests()
    {
        fileSystemWatcherFactoryMock
            .Setup(x => x.Create(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(fileSystemWatcherMock.Object);
    }

    [Fact]
    public async Task ShouldEnsureDestinationFolderExists()
    {
        var fileSystem = new MockFileSystem();
        fileSystem.AddDirectory("C:\\Watch");

        settingsMock.Setup(x => x.WatchFolder).Returns(@"C:\Watch");
        settingsMock.Setup(x => x.DestinationFolder).Returns(@"C:\Destination");
        settingsMock.Setup(x => x.PollingIntervalSeconds).Returns(300);

        var watcher = new FolderWatcher(
            loggerMock.Object,
            settingsMock.Object,
            fileSystem,
            fileScannerMock.Object,
            fileSystemWatcherFactoryMock.Object,
            commandHandlerMock.Object);

        await watcher
            .StartAsync(CancellationToken.None);

        Assert.True(
            fileSystem.FileExists(@"C:\Destination"));
    }

    [Fact]
    public async Task ShouldNotThrowWhenFolderAlreadyExists()
    {
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { @"C:\Destination\", new MockDirectoryData() }
        });

        settingsMock.Setup(x => x.WatchFolder).Returns(@"C:\Watch");
        settingsMock.Setup(x => x.DestinationFolder).Returns(@"C:\Destination");
        settingsMock.Setup(x => x.PollingIntervalSeconds).Returns(300);

        var watcher = new FolderWatcher(
            loggerMock.Object,
            settingsMock.Object,
            fileSystem,
            fileScannerMock.Object,
            fileSystemWatcherFactoryMock.Object,
            commandHandlerMock.Object);

        var exception = await Record.ExceptionAsync(
            () => watcher.StartAsync(CancellationToken.None));

        Assert.Null(exception);
    }
}
