namespace SendHub;

/// <summary>
/// Scans directories for files matching specific criteria.
/// </summary>
public interface IFileScanner
{
    /// <summary>
    /// Gets all files in the specified directory.
    /// </summary>
    Task<IReadOnlyList<FileInfo>> GetFiles(
        string directoryPath,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Gets files matching a specific pattern.
    /// </summary>
    Task<IReadOnlyList<FileInfo>> GetFiles(
        string directoryPath,
        string searchPattern,
        CancellationToken cancellationToken = default);
}
