using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using NodaTime.Testing;
using SendHub.Infrastructure.Database;
using SendHub.Infrastructure.Tracking;

namespace SendHub.Infrastructure.Tests.Tracking;

public sealed class SqliteFileTrackerTests : IDisposable
{
    private static readonly DateTimeOffset _now = DateTimeOffset.Parse("2024-01-01T12:00:00Z");
    private readonly string _tempDbPath;
    private readonly FakeClock _clock;

    public SqliteFileTrackerTests()
    {
        // Use a temp file for each test to avoid conflicts
        _tempDbPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
        _clock = new FakeClock(Instant.FromDateTimeOffset(_now));
    }

    public void Dispose()
    {
        // Force SQLite to release all connections
        SqliteConnection.ClearAllPools();
        GC.Collect();
        GC.WaitForPendingFinalizers();

        // Cleanup temp database after each test
        if (File.Exists(_tempDbPath))
        {
            try { File.Delete(_tempDbPath); } catch { /* Ignore */ }
        }

        // Clean up SQLite journal files
        var journalPath = _tempDbPath + "-journal";
        if (File.Exists(journalPath))
        {
            try { File.Delete(journalPath); } catch { /* Ignore */ }
        }

        var walPath = _tempDbPath + "-wal";
        if (File.Exists(walPath))
        {
            try { File.Delete(walPath); } catch { /* Ignore */ }
        }

        var shmPath = _tempDbPath + "-shm";
        if (File.Exists(shmPath))
        {
            try { File.Delete(shmPath); } catch { /* Ignore */ }
        }

        var backupPath = _tempDbPath + ".bak";
        if (File.Exists(backupPath))
        {
            try { File.Delete(backupPath); } catch { /* Ignore */ }
        }

        var jsonPath = Path.ChangeExtension(_tempDbPath, ".json");
        if (File.Exists(jsonPath))
        {
            try { File.Delete(jsonPath); } catch { /* Ignore */ }
        }

        var jsonBackupPath = jsonPath + ".bak";
        if (File.Exists(jsonBackupPath))
        {
            try { File.Delete(jsonBackupPath); } catch { /* Ignore */ }
        }
    }

    [Fact]
    public async Task GivenFileNeverTracked_WhenWasProcessed_ThenShouldReturnFalse()
    {
        using var tracker = CreateTracker();

        var result = await tracker.WasProcessed(@"C:\Watch\file.txt");

        Assert.False(result);
    }

    [Fact]
    public async Task GivenFileMarkedAsProcessed_WhenWasProcessed_ThenShouldReturnTrue()
    {
        const string filePath = @"C:\Watch\file.txt";
        using var tracker = CreateTracker();

        await tracker.MarkProcessed(filePath);
        var result = await tracker.WasProcessed(filePath);

        Assert.True(result);
    }

    [Fact]
    public async Task GivenMultipleFiles_WhenMarkProcessed_ThenShouldPersistAll()
    {
        using var tracker = CreateTracker();

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
        using var tracker = CreateTracker();

        // Mark same file twice
        await tracker.MarkProcessed(@"C:\Watch\file.txt");
        await tracker.MarkProcessed(@"C:\Watch\file.txt");

        // Verify only one entry exists in database
        using var connection = new SqliteConnection($"Data Source={_tempDbPath}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM processed_files";
        var count = (long)command.ExecuteScalar()!;

        Assert.Equal(1, count);
    }

    [Fact]
    public async Task GivenSameFileWithDifferentCasing_WhenWasProcessed_ThenShouldBeMarkedAsProcessed()
    {
        using var tracker = CreateTracker();

        await tracker.MarkProcessed(@"C:\Watch\Report.PDF");

        // Different casing - should still match due to path normalization
        Assert.True(await tracker.WasProcessed(@"C:\Watch\report.pdf"));
    }

    [Fact]
    public async Task GivenLegacyJsonExists_WhenInitialized_ThenShouldMigrateData()
    {
        // Create legacy JSON file
        var legacyJsonPath = Path.ChangeExtension(_tempDbPath, ".json");
        var existingRecords = new[]
        {
            new TrackedFile(@"C:\Watch\file1.txt", Instant.FromDateTimeOffset(_now)),
            new TrackedFile(@"C:\Watch\file2.txt", Instant.FromDateTimeOffset(_now))
        };

        var json = JsonSerializer.Serialize(existingRecords, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb));

        File.WriteAllText(legacyJsonPath, json);

        // Initialize tracker - should auto-migrate
        using var tracker = CreateTracker();

        // Verify migration worked
        Assert.True(await tracker.WasProcessed(@"C:\Watch\file1.txt"));
        Assert.True(await tracker.WasProcessed(@"C:\Watch\file2.txt"));

        // Verify backup was created
        Assert.True(File.Exists(legacyJsonPath + ".bak"));
    }

    [Fact]
    public async Task GivenCorruptedLegacyJson_WhenInitialized_ThenShouldStartFresh()
    {
        // Create corrupted JSON file
        var legacyJsonPath = Path.ChangeExtension(_tempDbPath, ".json");
        File.WriteAllText(legacyJsonPath, "this is not valid json!!!");

        // Should not throw - just starts fresh
        using var tracker = CreateTracker();

        var result = await tracker.WasProcessed(@"C:\Watch\file.txt");

        Assert.False(result);
    }

    [Fact]
    public async Task GivenDatabaseExists_WhenInitialized_ThenShouldReuseExisting()
    {
        // Create first tracker and add data
        using (var tracker1 = CreateTracker())
        {
            await tracker1.MarkProcessed(@"C:\Watch\file1.txt");
        }

        // Create second tracker with same database
        using var tracker2 = CreateTracker();

        // Should still have the data
        Assert.True(await tracker2.WasProcessed(@"C:\Watch\file1.txt"));
    }

    [Fact]
    public void GivenNewDatabase_WhenInitialized_ThenShouldCreateSchema()
    {
        using var tracker = CreateTracker();

        // Verify tables were created
        using var connection = new SqliteConnection($"Data Source={_tempDbPath}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = @"
            SELECT COUNT(*)
            FROM sqlite_master
            WHERE type='table' AND name IN ('processed_files', 'schema_version')";
        var tableCount = (long)command.ExecuteScalar()!;

        Assert.Equal(2, tableCount);
    }

    [Fact]
    public void GivenNewDatabase_WhenInitialized_ThenShouldSetSchemaVersion()
    {
        using var tracker = CreateTracker();

        // Verify schema version was set
        using var connection = new SqliteConnection($"Data Source={_tempDbPath}");
        connection.Open();

        using var command = connection.CreateCommand();
        command.CommandText = "SELECT version FROM schema_version";
        var version = (long)command.ExecuteScalar()!;

        Assert.Equal(1, version);
    }

    private SqliteFileTracker CreateTracker()
    {
        var settings = Options.Create(new DatabaseSettings { Path = _tempDbPath });
        var logger = new Mock<ILogger<SqliteFileTracker>>().Object;

        return new SqliteFileTracker(settings, _clock, logger);
    }
}
