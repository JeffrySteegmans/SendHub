namespace SendHub;

public interface IFileArchiver
{
    Task Archive(
        FileInfo file,
        CancellationToken cancellationToken);
}
