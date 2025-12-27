namespace SendHub.Features.Folder.Scan;

public record ScanFolder(
    string Path) : ICommand;
