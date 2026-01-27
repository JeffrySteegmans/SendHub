namespace SendHub;

/// <summary>
/// Abstraction for monitoring file system changes.
/// </summary>
public interface IFileSystemWatcher
    : IDisposable
{
    /// <summary>
    /// Occurs when a file or directory is created.
    /// </summary>
    event FileSystemEventHandler? Created;

    /// <summary>
    /// Occurs when the watcher encounters an error.
    /// </summary>
    event ErrorEventHandler? Error;

    /// <summary>
    /// Gets or sets a value indicating whether the watcher is enabled.
    /// </summary>
    bool EnableRaisingEvents { get; set; }
}
