using System.IO.Abstractions;
using Microsoft.Extensions.Logging;

namespace SendHub.Infrastructure.FileSystem;

internal sealed class FileArchiver(
    IFileSystem fileSystem,
    ILogger<FileArchiver> logger) : IFileArchiver
{
    public Task Archive(
        FileInfo file,
        string destinationFolder,
        CancellationToken cancellationToken)
    {
        if (!fileSystem.Directory.Exists(destinationFolder))
            fileSystem.Directory.CreateDirectory(destinationFolder);

        var destination = ResolveDestinationPath(file, destinationFolder);
        fileSystem.File.Move(file.FullName, destination);
        LogMessages.FileArchived(logger, file.Name, destination);
        return Task.CompletedTask;
    }

    private string ResolveDestinationPath(FileInfo file, string destinationFolder)
    {
        var nameWithoutExt = Path.GetFileNameWithoutExtension(file.Name);
        var ext = file.Extension;
        var candidate = Path.Combine(destinationFolder, file.Name);

        var counter = 0;
        while (fileSystem.File.Exists(candidate))
        {
            counter++;
            candidate = Path.Combine(destinationFolder, $"{nameWithoutExt}_{counter}{ext}");
        }

        return candidate;
    }
}
