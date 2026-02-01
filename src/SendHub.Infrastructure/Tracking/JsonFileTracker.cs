using System.IO.Abstractions;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NodaTime;

namespace SendHub.Infrastructure.Tracking;

internal sealed class JsonFileTracker(
    IFileSystem fileSystem,
    IClock clock,
    IOptions<TrackingSettings> settings,
    ILogger<JsonFileTracker> logger) : IProcessedFileTracker
{
    private static readonly JsonSerializerOptions JsonOptions = new ()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    private readonly string _trackingFilePath = settings.Value.FilePath;
    private readonly SemaphoreSlim _lock = new(1, 1);

    /// <summary>
    /// In-memory cache. Null until first load; never null after.
    /// Key: normalized full path. Value: when it was processed.
    /// </summary>
    private Dictionary<string, Instant>? _entries;

    public async Task<bool> WasProcessed(
        string filePath)
    {
        await _lock.WaitAsync();
        try
        {
            await EnsureLoadedAsync();
            return _entries!.ContainsKey(NormalizePath(filePath));
        }
        finally
        {
            _lock.Release();
        }
    }

    public async Task MarkProcessed(
        string filePath)
    {
        await _lock.WaitAsync();
        try
        {
            await EnsureLoadedAsync();

            _entries![NormalizePath(filePath)] = clock.GetCurrentInstant();

            await PersistAsync();

            LogMessages
                .MarkedAsProcessed(logger, filePath);
        }
        finally
        {
            _lock.Release();
        }
    }

    /// <summary>
    /// Loads entries from disk on first access. No-op on subsequent calls.
    /// Must be called while holding _lock.
    /// </summary>
    private async Task EnsureLoadedAsync()
    {
        if (_entries is not null)
            return;

        _entries = new Dictionary<string, Instant>(
            StringComparer.OrdinalIgnoreCase);

        if (!fileSystem.File.Exists(_trackingFilePath))
        {
            LogMessages
                .NoTrackingFile(logger, _trackingFilePath);
            return;
        }

        try
        {
            await using var stream = fileSystem.File
                .OpenRead(_trackingFilePath);
            var trackedFiles = await JsonSerializer
                .DeserializeAsync<TrackedFile[]>(stream, JsonOptions);

            if (trackedFiles is null)
                return;

            foreach (var trackedFile in trackedFiles)
            {
                _entries[trackedFile.FilePath] = trackedFile.ProcessedAt;
            }

            LogMessages
                .TrackedFilesLoaded(logger, _entries.Count, _trackingFilePath);
        }
        catch (JsonException ex)
        {
            // Corrupted file - start fresh rather than crashing
            LogMessages
                .FileCorrupted(logger, _trackingFilePath, ex);
            _entries.Clear();
        }
    }

    /// <summary>
    /// Writes current state to disk atomically: write to temp file, then replace.
    /// Must be called while holding _lock.
    /// </summary>
    private async Task PersistAsync()
    {
        var tempPath = _trackingFilePath + ".tmp";

        var records = _entries!
            .Select(kvp => new TrackedFile(kvp.Key, kvp.Value))
            .ToArray();

        await using (var stream = fileSystem.File.Create(tempPath))
        {
            await JsonSerializer.SerializeAsync(stream, records, JsonOptions);
        }

        // Atomic replacement - overwrites target in a single operation
        fileSystem.File
            .Move(tempPath, _trackingFilePath, overwrite: true);
    }

    /// <summary>
    /// Normalizes path so that "C:\Watch\file.txt" and "C:\watch\FILE.TXT"
    /// are treated as the same file.
    /// </summary>
    private static string NormalizePath(string path)
        => Path.GetFullPath(path);
}
