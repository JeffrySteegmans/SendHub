using System.IO.Abstractions.TestingHelpers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using SendHub.Features.FileProcessing;

namespace SendHub.Daemon.Tests.FolderWatcherTests;

public sealed class StartTests
{
    private readonly Mock<ILogger<FolderWatcher>> loggerMock = new ();
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

        var settings = Options.Create(new FolderWatcherSettings
        {
            WatchFolder = @"C:\Watch",
            DestinationFolder = @"C:\Destination"
        });

        var watcher = new FolderWatcher(
            loggerMock.Object,
            settings,
            fileSystem,
            fileScannerMock.Object,
            fileSystemWatcherFactoryMock.Object,
            commandHandlerMock.Object);

        await watcher
            .StartAsync(CancellationToken.None);

        Assert.True(
            fileSystem.FileExists(settings.Value.DestinationFolder));
    }

    [Fact]
    public async Task ShouldNotThrowWhenFolderAlreadyExists()
    {
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { @"C:\Destination\", new MockDirectoryData() }
        });

        var settings = Options.Create(new FolderWatcherSettings
        {
            WatchFolder = @"C:\Watch",
            DestinationFolder = @"C:\Destination"
        });

        var watcher = new FolderWatcher(
            loggerMock.Object,
            settings,
            fileSystem,
            fileScannerMock.Object,
            fileSystemWatcherFactoryMock.Object,
            commandHandlerMock.Object);

        var exception = await Record.ExceptionAsync(
            () => watcher.StartAsync(CancellationToken.None));

        Assert.Null(exception);
    }
}
