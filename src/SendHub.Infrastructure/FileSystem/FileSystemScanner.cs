namespace SendHub.Infrastructure.FileSystem;

internal sealed class FileSystemScanner : IFileScanner
{
    public Task<IReadOnlyList<FileInfo>> GetFiles(
        string directoryPath,
        CancellationToken cancellationToken = default)
    {
        return GetFiles(directoryPath, "*.*", cancellationToken);
    }

    public Task<IReadOnlyList<FileInfo>> GetFiles(
        string directoryPath,
        string searchPattern,
        CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(directoryPath))
        {
            return Task.FromResult<IReadOnlyList<FileInfo>>(
                []);
        }

        var directory = new DirectoryInfo(directoryPath);
        var files = directory
            .GetFiles(searchPattern, SearchOption.TopDirectoryOnly)
            .Where(f => !f.Attributes.HasFlag(FileAttributes.Hidden))
            .ToList();

        return Task.FromResult<IReadOnlyList<FileInfo>>(files);
    }
}
