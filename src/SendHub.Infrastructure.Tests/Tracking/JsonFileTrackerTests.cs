using System.IO.Abstractions;
using System.IO.Abstractions.TestingHelpers;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using NodaTime.Testing;
using SendHub.Infrastructure.Tracking;

namespace SendHub.Infrastructure.Tests.Tracking;

public sealed class JsonFileTrackerTests
{
    private const string TrackingPath = @"C:\Temp\tracking.json";
    private static readonly DateTimeOffset _now = DateTimeOffset.Parse("2024-01-01T12:00:00Z");

    [Fact]
    public async Task GivenFileNeverTracked_WhenWasProcessed_ThenShouldReturnFalse()
    {
        var tracker = CreateTracker(
            new MockFileSystem());

        var result = await tracker
            .WasProcessed(@"C:\Watch\file.txt");

        Assert
            .False(result);
    }

    [Fact]
    public async Task GivenFileMarkedAsProcessed_WhenWasProcessed_ThenShouldReturnTrue()
    {
        const string filePath = @"C:\Watch\file.txt";
        var tracker = CreateTracker(
            new MockFileSystem());

        await tracker
            .MarkProcessed(filePath);
        var result = await tracker
            .WasProcessed(filePath);

        Assert
            .True(result);
    }

    [Fact]
    public async Task GivenLoadedFromDisk_WhenWasProcessed_ThenShouldReturnTrue()
    {
        var existingRecords = new[]
        {
            new TrackedFile(@"C:\Watch\file1.txt", Instant.FromDateTimeOffset(_now)),
            new TrackedFile(@"C:\Watch\file2.txt", Instant.FromDateTimeOffset(_now))
        };

        var json = JsonSerializer.Serialize(existingRecords, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb));

        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { TrackingPath, new MockFileData(json) }
        });

        var tracker = CreateTracker(
            fileSystem);

        Assert
            .True(await tracker.WasProcessed(@"C:\Watch\file1.txt"));
        Assert
            .True(await tracker.WasProcessed(@"C:\Watch\file2.txt"));
    }

    [Fact]
    public async Task givenFileMarkedAsProcessed_ThenShouldPersistToFile()
    {
        var fileSystem = new MockFileSystem();
        var tracker = CreateTracker(fileSystem);

        await tracker
            .MarkProcessed(@"C:\Watch\file.txt");

        // Verify file was written
        Assert
            .True(fileSystem.File.Exists(TrackingPath));

        var json = await fileSystem.File
            .ReadAllTextAsync(TrackingPath);
        var trackedFiles = JsonSerializer.Deserialize<TrackedFile[]>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb));

        Assert
            .NotNull(trackedFiles);
        var trackedFile = Assert
            .Single(trackedFiles);
        Assert
            .Equal(@"C:\Watch\file.txt", trackedFile.FilePath);
    }

    [Fact]
    public async Task GivenSameFileWithDifferentCasing_WhenWasProcessed_ThenShouldBeMarkedAsProcessed()
    {
        var tracker = CreateTracker(new MockFileSystem());

        await tracker
            .MarkProcessed(@"C:\Watch\Report.PDF");

        // Different casing - should still match
        Assert
            .True(await tracker.WasProcessed(@"C:\Watch\report.pdf"));
    }

    [Fact]
    public async Task GivenCorruptedTrackingFile_WhenWasProcessedThenShouldStartFresh()
    {
        var fileSystem = new MockFileSystem(new Dictionary<string, MockFileData>
        {
            { TrackingPath, new MockFileData("this is not valid json!!!") }
        });

        var tracker = CreateTracker(fileSystem);

        // Should not throw - just starts fresh
        var result = await tracker
            .WasProcessed(@"C:\Watch\file.txt");

        Assert
            .False(result);
    }

    [Fact]
    public async Task GivenMultipleFiles_WhenMarkProcessed_ThenShouldPersistsAll()
    {
        var fileSystem = new MockFileSystem();
        var tracker = CreateTracker(fileSystem);

        await tracker.MarkProcessed(@"C:\Watch\file1.txt");
        await tracker.MarkProcessed(@"C:\Watch\file2.txt");
        await tracker.MarkProcessed(@"C:\Watch\file3.txt");

        Assert.True(await tracker.WasProcessed(@"C:\Watch\file1.txt"));
        Assert.True(await tracker.WasProcessed(@"C:\Watch\file2.txt"));
        Assert.True(await tracker.WasProcessed(@"C:\Watch\file3.txt"));
    }

    [Fact]
    public async Task GivenDuplicateFile_WhenMarkProcessed_ThenShouldNotCreateDuplicateEntries()
    {
        var fileSystem = new MockFileSystem();
        var tracker = CreateTracker(fileSystem);

        // Mark same file twice
        await tracker.MarkProcessed(@"C:\Watch\file.txt");
        await tracker.MarkProcessed(@"C:\Watch\file.txt");

        var json = await fileSystem.File.ReadAllTextAsync(TrackingPath);
        var records = JsonSerializer.Deserialize<TrackedFile[]>(json, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb));

        Assert
            .NotNull(records);
        Assert
            .Single(records);
    }

    private static JsonFileTracker CreateTracker(
        IFileSystem fileSystem)
    {
        return new JsonFileTracker(
            fileSystem,
            new FakeClock(Instant.FromDateTimeOffset(_now)),
            Options.Create(new TrackingSettings { FilePath = TrackingPath }),
            Mock.Of<ILogger<JsonFileTracker>>());
    }
}
