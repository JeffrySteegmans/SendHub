namespace SendHub.Infrastructure.FileSystem;

/// <summary>
/// Factory for creating FileSystemWatcher instances.
/// </summary>
internal sealed class FileSystemWatcherFactory : IFileSystemWatcherFactory
{
    public IFileSystemWatcher Create(
        string path,
        string filter)
    {
        return new FileSystemWatcherAdapter(
            path,
            filter);
    }
}
