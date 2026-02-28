namespace SendHub;

public interface IFileSender
{
    string Name { get; }

    Task Send(
        FileInfo filePath,
        CancellationToken cancellationToken);
}
