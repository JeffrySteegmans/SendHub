namespace SendHub.Infrastructure.FileSystem;

internal sealed class FileArchiver
    : IFileArchiver
{
    public Task Archive(
        FileInfo file,
        CancellationToken cancellationToken)
    {
        //TODO: Implement archiving logic
        return Task.CompletedTask;
    }
}
