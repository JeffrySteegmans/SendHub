namespace SendHub;

/// <summary>
/// Factory for creating file system watchers.
/// </summary>
public interface IFileSystemWatcherFactory
{
    /// <summary>
    /// Creates a file system watcher for the specified path and filter.
    /// </summary>
    IFileSystemWatcher Create(
        string path,
        string filter);
}
