namespace SendHub.Infrastructure.Messaging;

public interface IFileSender
{
    string Name { get; }
    Task Send(FileInfo file, CancellationToken ct);
}
