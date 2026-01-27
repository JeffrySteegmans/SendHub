namespace SendHub.Daemon.Tests.Fakes;

internal sealed class FakeFileSystemWatcher : IFileSystemWatcher
{
    public event FileSystemEventHandler? Created;
    public event ErrorEventHandler? Error;

    public bool EnableRaisingEvents { get; set; }

    public void RaiseCreated(FileSystemEventArgs args)
    {
        Created?
            .Invoke(this, args);
    }

    public void Dispose()
    {
    }
}
