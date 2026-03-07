using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using SendHub.Infrastructure.Database;
using SendHub.Infrastructure.Settings;

namespace SendHub.Infrastructure.Tests.Settings;

public sealed class SqliteApplicationSettingsTests : IDisposable
{
    private readonly string _tempDbPath;

    public SqliteApplicationSettingsTests()
    {
        _tempDbPath = Path.Combine(Path.GetTempPath(), $"test_{Guid.NewGuid()}.db");
    }

    public void Dispose()
    {
        SqliteConnection.ClearAllPools();
        GC.Collect();
        GC.WaitForPendingFinalizers();

        foreach (var suffix in new[] { "", "-journal", "-wal", "-shm" })
        {
            var path = _tempDbPath + suffix;
            if (File.Exists(path))
                try { File.Delete(path); } catch { /* Ignore */ }
        }
    }

    [Fact]
    public void GivenNewDatabase_WhenInitialized_ThenCreatesSettingsTable()
    {
        using var sut = CreateSettings();

        using var connection = new SqliteConnection($"Data Source={_tempDbPath}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='settings'";
        var count = (long)command.ExecuteScalar()!;

        Assert.Equal(1, count);
    }

    [Fact]
    public void GivenNewDatabase_WhenInitialized_ThenSeedsAllDefaults()
    {
        using var sut = CreateSettings();

        using var connection = new SqliteConnection($"Data Source={_tempDbPath}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM settings";
        var count = (long)command.ExecuteScalar()!;

        Assert.Equal(10, count);
    }

    [Fact]
    public async Task GivenExistingSettings_WhenInitialized_ThenLoadsFromDatabase()
    {
        // Seed a custom value via first instance
        using (var first = CreateSettings())
        {
            await first.UpdateAsync(SettingKeys.SmtpHost, "custom.smtp.com");
        }

        // Second instance should load custom value from DB
        using var sut = CreateSettings();

        Assert.Equal("custom.smtp.com", sut.SmtpHost);
    }

    [Fact]
    public async Task GivenPopulatedTable_WhenInitialized_ThenDoesNotOverwriteExisting()
    {
        using (var first = CreateSettings())
        {
            await first.UpdateAsync(SettingKeys.SmtpHost, "custom.smtp.com");
        }

        // Reinitialize — seeding with INSERT OR IGNORE must not overwrite
        using var sut = CreateSettings();

        Assert.Equal("custom.smtp.com", sut.SmtpHost);
    }

    [Fact]
    public async Task GivenSettings_WhenUpdateAsync_ThenPersistsToDatabase()
    {
        using (var sut = CreateSettings())
        {
            await sut.UpdateAsync(SettingKeys.SmtpPort, "465");
        }

        // Verify persisted by creating new instance
        using var sut2 = CreateSettings();
        Assert.Equal(465, sut2.SmtpPort);
    }

    [Fact]
    public async Task GivenSettings_WhenUpdateAsync_ThenCacheUpdatedImmediately()
    {
        using var sut = CreateSettings();

        await sut.UpdateAsync(SettingKeys.SmtpHost, "new.smtp.com");

        Assert.Equal("new.smtp.com", sut.SmtpHost);
    }

    [Fact]
    public void GivenNewInstance_WhenInitializedTwice_ThenIdempotent()
    {
        using (var first = CreateSettings()) { }

        // Second init should not throw or create duplicates
        using var sut = CreateSettings();

        using var connection = new SqliteConnection($"Data Source={_tempDbPath}");
        connection.Open();
        using var command = connection.CreateCommand();
        command.CommandText = "SELECT COUNT(*) FROM settings";
        var count = (long)command.ExecuteScalar()!;

        Assert.Equal(10, count);
    }

    private SqliteApplicationSettings CreateSettings()
    {
        var options = Options.Create(new DatabaseSettings { Path = _tempDbPath });
        return new SqliteApplicationSettings(options);
    }
}
