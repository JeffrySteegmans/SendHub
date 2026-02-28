namespace SendHub.Features.FileProcessing;

public record ProcessIncomingFile(
    FileInfo File,
    string DestinationFolder) : ICommand;
