using System.Text.Json;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;
using NodaTime.Serialization.SystemTextJson;
using SendHub.Infrastructure.Database;

namespace SendHub.Infrastructure.Tracking;

internal sealed class SqliteFileTracker : IProcessedFileTracker, IDisposable
{
    private static readonly JsonSerializerOptions JsonOptions = new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    }.ConfigureForNodaTime(DateTimeZoneProviders.Tzdb);

    private readonly SqliteConnection _connection;
    private readonly IClock _clock;
    private readonly ILogger<SqliteFileTracker> _logger;
    private readonly SemaphoreSlim _lock = new(1, 1);

    public SqliteFileTracker(
        IOptions<DatabaseSettings> settings,
        IClock clock,
        ILogger<SqliteFileTracker> logger)
    {
        _clock = clock;
        _logger = logger;

        var connectionString = $"Data Source={settings.Value.Path}";
        _connection = new SqliteConnection(connectionString);

        EnsureInitialized(settings.Value.Path);
    }

    public async Task<bool> WasProcessed(string filePath)
    {
        await _lock.WaitAsync();
        try
        {
            var normalizedPath = NormalizePath(filePath);

            var count = await _connection.ExecuteScalarAsync<int>(
                "SELECT COUNT(*) FROM processed_files WHERE file_path = @FilePath",
                new { FilePath = normalizedPath });

            return count > 0;
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task MarkProcessed(string filePath)
    {
        await _lock.WaitAsync();
        try
        {
            var normalizedPath = NormalizePath(filePath);
            var processedAt = FormatInstant(_clock.GetCurrentInstant());

            await _connection.ExecuteAsync(
                "INSERT OR REPLACE INTO processed_files (file_path, processed_at) VALUES (@FilePath, @ProcessedAt)",
                new { FilePath = normalizedPath, ProcessedAt = processedAt });

            LogMessages.MarkedAsProcessed(_logger, filePath);
        }
        finally
        {
            _lock.Release();
        }
    }

    private void EnsureInitialized(string dbPath)
    {
        _connection.Open();

        var tablesExist = _connection.ExecuteScalar<int>(
            "SELECT COUNT(*) FROM sqlite_master WHERE type='table' AND name='processed_files'") > 0;

        if (!tablesExist)
        {
            CreateSchema();
            MigrateLegacyJsonIfExists(dbPath);
        }
    }

    private void CreateSchema()
    {
        _connection.Execute(@"
            CREATE TABLE processed_files (
                file_path TEXT NOT NULL PRIMARY KEY,
                processed_at TEXT NOT NULL
            );

            CREATE TABLE schema_version (
                version INTEGER NOT NULL,
                applied_at TEXT NOT NULL
            );
        ");

        var appliedAt = FormatInstant(_clock.GetCurrentInstant());
        _connection.Execute(
            "INSERT INTO schema_version (version, applied_at) VALUES (@Version, @AppliedAt)",
            new { Version = 1, AppliedAt = appliedAt });

        LogMessages.SchemaCreated(_logger, 1);
    }

    private void MigrateLegacyJsonIfExists(string dbPath)
    {
        // Infer legacy JSON path: same directory as DB, with .json extension
        var legacyJsonPath = Path.ChangeExtension(dbPath, ".json");

        if (!File.Exists(legacyJsonPath))
        {
            LogMessages.NoLegacyJsonFound(_logger, legacyJsonPath);
            return;
        }

        try
        {
            var json = File.ReadAllText(legacyJsonPath);
            var trackedFiles = JsonSerializer.Deserialize<TrackedFile[]>(json, JsonOptions);

            if (trackedFiles == null || trackedFiles.Length == 0)
            {
                LogMessages.NoLegacyJsonFound(_logger, legacyJsonPath);
                return;
            }

            // Batch insert using transaction
            using var transaction = _connection.BeginTransaction();

            foreach (var file in trackedFiles)
            {
                _connection.Execute(
                    "INSERT OR IGNORE INTO processed_files (file_path, processed_at) VALUES (@FilePath, @ProcessedAt)",
                    new { FilePath = NormalizePath(file.FilePath), ProcessedAt = FormatInstant(file.ProcessedAt) },
                    transaction);
            }

            transaction.Commit();

            // Backup legacy JSON
            File.Move(legacyJsonPath, legacyJsonPath + ".bak", overwrite: true);

            LogMessages.MigratedFromJson(_logger, trackedFiles.Length, legacyJsonPath);
        }
        catch (Exception ex)
        {
            LogMessages.MigrationFailed(_logger, legacyJsonPath, ex);
            // Continue - don't crash on migration failure
        }
    }

    private static string NormalizePath(string path)
        => Path.GetFullPath(path).ToUpperInvariant();

    private static string FormatInstant(Instant instant)
        => instant.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'", null);

    public void Dispose()
    {
        try
        {
            _connection?.Close();
            _connection?.Dispose();
        }
        catch
        {
            // Ignore disposal errors
        }
        finally
        {
            _lock?.Dispose();
        }
    }
}
