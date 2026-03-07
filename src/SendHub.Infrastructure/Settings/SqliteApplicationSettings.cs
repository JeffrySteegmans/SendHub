using System.Collections.Concurrent;
using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Options;
using SendHub.Infrastructure.Database;

namespace SendHub.Infrastructure.Settings;

internal sealed class SqliteApplicationSettings : IApplicationSettings, IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly SemaphoreSlim _lock = new(1, 1);
    private readonly ConcurrentDictionary<string, string> _cache = new();

    public SqliteApplicationSettings(IOptions<DatabaseSettings> settings)
    {
        var connectionString = $"Data Source={settings.Value.Path}";
        _connection = new SqliteConnection(connectionString);
        EnsureInitialized();
    }

    public string WatchFolder => _cache[SettingKeys.WatchFolder];
    public string DestinationFolder => _cache[SettingKeys.DestinationFolder];
    public int PollingIntervalSeconds => int.Parse(_cache[SettingKeys.PollingIntervalSeconds]);
    public string SmtpHost => _cache[SettingKeys.SmtpHost];
    public int SmtpPort => int.Parse(_cache[SettingKeys.SmtpPort]);
    public string? SmtpUsername => _cache.TryGetValue(SettingKeys.SmtpUsername, out var u) ? u : null;
    public string? SmtpPassword => _cache.TryGetValue(SettingKeys.SmtpPassword, out var p) ? p : null;
    public bool SmtpEnableSsl => bool.Parse(_cache[SettingKeys.SmtpEnableSsl]);
    public string SmtpFrom => _cache[SettingKeys.SmtpFrom];
    public string SmtpTo => _cache[SettingKeys.SmtpTo];

    public async Task UpdateAsync(string key, string value, CancellationToken cancellationToken = default)
    {
        await _lock.WaitAsync(cancellationToken);
        try
        {
            var updatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
            await _connection.ExecuteAsync(
                "INSERT OR REPLACE INTO settings (key, value, updated_at) VALUES (@Key, @Value, @UpdatedAt)",
                new { Key = key, Value = value, UpdatedAt = updatedAt });

            _cache[key] = value;
        }
        finally
        {
            _lock.Release();
        }
    }

    private void EnsureInitialized()
    {
        _connection.Open();

        _connection.Execute(@"
            CREATE TABLE IF NOT EXISTS settings (
                key        TEXT NOT NULL PRIMARY KEY,
                value      TEXT NOT NULL,
                updated_at TEXT NOT NULL
            )");

        SeedDefaults();

        var rows = _connection.Query<SettingRow>("SELECT key, value FROM settings");
        foreach (var row in rows)
            _cache[row.Key] = row.Value;
    }

    private void SeedDefaults()
    {
        var defaults = new Dictionary<string, string>
        {
            [SettingKeys.WatchFolder]            = @"D:\SendHub\ScanFolder",
            [SettingKeys.DestinationFolder]      = @"D:\SendHub\ScanFolder\Processed",
            [SettingKeys.PollingIntervalSeconds]  = "30",
            [SettingKeys.SmtpHost]               = "smtp.gmail.com",
            [SettingKeys.SmtpPort]               = "587",
            [SettingKeys.SmtpUsername]           = "your-email@gmail.com",
            [SettingKeys.SmtpPassword]           = "your-app-password",
            [SettingKeys.SmtpEnableSsl]          = "true",
            [SettingKeys.SmtpFrom]               = "sendhub@example.com",
            [SettingKeys.SmtpTo]                 = "recipient@example.com",
        };

        var updatedAt = DateTime.UtcNow.ToString("yyyy-MM-dd'T'HH:mm:ss'Z'");
        using var transaction = _connection.BeginTransaction();
        foreach (var (key, value) in defaults)
        {
            _connection.Execute(
                "INSERT OR IGNORE INTO settings (key, value, updated_at) VALUES (@Key, @Value, @UpdatedAt)",
                new { Key = key, Value = value, UpdatedAt = updatedAt },
                transaction);
        }
        transaction.Commit();
    }

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

    private record SettingRow(string Key, string Value);
}

internal static class SettingKeys
{
    public const string WatchFolder            = "SendHub:WatchFolder";
    public const string DestinationFolder      = "SendHub:DestinationFolder";
    public const string PollingIntervalSeconds  = "SendHub:PollingIntervalSeconds";
    public const string SmtpHost               = "SendHub:Email:Smtp:Host";
    public const string SmtpPort               = "SendHub:Email:Smtp:Port";
    public const string SmtpUsername           = "SendHub:Email:Smtp:Username";
    public const string SmtpPassword           = "SendHub:Email:Smtp:Password";
    public const string SmtpEnableSsl          = "SendHub:Email:Smtp:EnableSsl";
    public const string SmtpFrom               = "SendHub:Email:Smtp:From";
    public const string SmtpTo                 = "SendHub:Email:Smtp:To";
}
