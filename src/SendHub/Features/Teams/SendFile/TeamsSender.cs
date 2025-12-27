namespace SendHub.Features.Teams.SendFile;

internal sealed class TeamsSender
    : IFileSender
{
    public string Name
        => "Teams";

    public Task Send(
        FileInfo filePath,
        CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }
}
