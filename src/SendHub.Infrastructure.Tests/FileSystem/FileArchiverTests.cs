using System.IO.Abstractions.TestingHelpers;
using Microsoft.Extensions.Logging;
using Moq;
using SendHub.Infrastructure.FileSystem;

namespace SendHub.Infrastructure.Tests.FileSystem;

public sealed class FileArchiverTests
{
    private const string WatchFolder = @"C:\Watch";
    private const string DestinationFolder = @"C:\Processed";

    [Fact]
    public async Task GivenFile_WhenArchive_ThenFileShouldBeMovedToDestination()
    {
        const string fileName = "report.pdf";
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { @$"{WatchFolder}\{fileName}", new MockFileData("content") }
        });

        var archiver = CreateArchiver(fileSystem);
        var file = new FileInfo(@$"{WatchFolder}\{fileName}");

        await archiver.Archive(file, DestinationFolder, CancellationToken.None);

        Assert.True(fileSystem.File.Exists(@$"{DestinationFolder}\{fileName}"));
    }

    [Fact]
    public async Task GivenFile_WhenArchive_ThenSourceFileShouldNoLongerExist()
    {
        const string fileName = "report.pdf";
        var sourcePath = @$"{WatchFolder}\{fileName}";
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { sourcePath, new MockFileData("content") }
        });

        var archiver = CreateArchiver(fileSystem);
        var file = new FileInfo(sourcePath);

        await archiver.Archive(file, DestinationFolder, CancellationToken.None);

        Assert.False(fileSystem.File.Exists(sourcePath));
    }

    [Fact]
    public async Task GivenFileWithSameNameAlreadyExists_WhenArchive_ThenShouldMoveWithCounterSuffix()
    {
        const string fileName = "report.pdf";
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { @$"{WatchFolder}\{fileName}", new MockFileData("new content") },
            { @$"{DestinationFolder}\{fileName}", new MockFileData("existing content") }
        });

        var archiver = CreateArchiver(fileSystem);
        var file = new FileInfo(@$"{WatchFolder}\{fileName}");

        await archiver.Archive(file, DestinationFolder, CancellationToken.None);

        Assert.True(fileSystem.File.Exists(@$"{DestinationFolder}\report_1.pdf"));
        Assert.True(fileSystem.File.Exists(@$"{DestinationFolder}\{fileName}"));
    }

    [Fact]
    public async Task GivenMultipleConflicts_WhenArchive_ThenShouldIncrementCounter()
    {
        const string fileName = "report.pdf";
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { @$"{WatchFolder}\{fileName}", new MockFileData("new content") },
            { @$"{DestinationFolder}\{fileName}", new MockFileData("existing") },
            { @$"{DestinationFolder}\report_1.pdf", new MockFileData("existing 1") },
            { @$"{DestinationFolder}\report_2.pdf", new MockFileData("existing 2") }
        });

        var archiver = CreateArchiver(fileSystem);
        var file = new FileInfo(@$"{WatchFolder}\{fileName}");

        await archiver.Archive(file, DestinationFolder, CancellationToken.None);

        Assert.True(fileSystem.File.Exists(@$"{DestinationFolder}\report_3.pdf"));
    }

    private static FileArchiver CreateArchiver(MockFileSystem fileSystem)
    {
        return new FileArchiver(
            fileSystem,
            Mock.Of<ILogger<FileArchiver>>());
    }
}
