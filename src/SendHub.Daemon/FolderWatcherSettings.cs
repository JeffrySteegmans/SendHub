namespace SendHub.Daemon;

public record FolderWatcherSettings
{
    public const string SectionName = "SendHub";

    public required string WatchFolder { get; init; }
    public required string DestinationFolder { get; init; }
}
