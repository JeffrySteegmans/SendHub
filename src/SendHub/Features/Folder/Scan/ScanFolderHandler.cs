namespace SendHub.Features.Folder.Scan;

internal sealed class ScanFolderHandler
    : ICommandHandler<ScanFolder, ScanFolderResult>
{
    public Task<ScanFolderResult> Handle(
        ScanFolder command,
        CancellationToken cancellationToken)
    {
        if (!Directory.Exists(command.Path))
            throw new DirectoryNotFoundException(command.Path);

        var files = Directory
            .GetFiles(command.Path, "*.*", SearchOption.TopDirectoryOnly)
            .Select(filePath => new FileInfo(filePath))
            .ToList();

        var result = new ScanFolderResult(
            files.Count,
            files);

        return Task.FromResult(
            result);
    }
}
