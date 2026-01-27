namespace SendHub.Infrastructure.FileSystem;

/// <summary>
/// Adapter that wraps System.IO.FileSystemWatcher to implement IFileSystemWatcher.
/// </summary>
internal sealed class FileSystemWatcherAdapter
    : IFileSystemWatcher
{
    private readonly FileSystemWatcher _watcher;
    private bool _disposed;

    public FileSystemWatcherAdapter(
        string path,
        string filter)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Path cannot be null or empty", nameof(path));

        _watcher = new FileSystemWatcher(path)
        {
            NotifyFilter = NotifyFilters.FileName | NotifyFilters.LastWrite,
            Filter = filter,
            EnableRaisingEvents = false
        };
    }

    public event FileSystemEventHandler? Created
    {
        add => _watcher.Created += value;
        remove => _watcher.Created -= value;
    }

    public event ErrorEventHandler? Error
    {
        add => _watcher.Error += value;
        remove => _watcher.Error -= value;
    }

    public bool EnableRaisingEvents
    {
        get => _watcher.EnableRaisingEvents;
        set => _watcher.EnableRaisingEvents = value;
    }

    public void Dispose()
    {
        if (_disposed) return;

        _watcher.Dispose();
        _disposed = true;
    }
}
