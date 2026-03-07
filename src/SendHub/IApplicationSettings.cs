namespace SendHub;

public interface IApplicationSettings
{
    string WatchFolder { get; }
    string DestinationFolder { get; }
    int PollingIntervalSeconds { get; }
    string SmtpHost { get; }
    int SmtpPort { get; }
    string? SmtpUsername { get; }
    string? SmtpPassword { get; }
    bool SmtpEnableSsl { get; }
    string SmtpFrom { get; }
    string SmtpTo { get; }

    Task UpdateAsync(string key, string value, CancellationToken cancellationToken = default);
}
