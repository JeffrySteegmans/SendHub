namespace SendHub.Features.Folder.Scan;

public record ScanFolderResult(
    int fileCount,
    IReadOnlyList<FileInfo> files);
