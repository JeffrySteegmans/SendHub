namespace SendHub;

public interface IFileArchiver
{
    Task Archive(
        FileInfo file,
        string destinationFolder,
        CancellationToken cancellationToken);
}
