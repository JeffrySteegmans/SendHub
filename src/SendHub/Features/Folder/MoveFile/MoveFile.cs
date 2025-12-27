namespace SendHub.Features.Folder.MoveFile;

public record MoveFile(
    FileInfo SourceFile,
    DirectoryInfo Destination) : ICommand;
