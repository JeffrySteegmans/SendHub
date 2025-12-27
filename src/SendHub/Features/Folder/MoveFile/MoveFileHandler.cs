namespace SendHub.Features.Folder.MoveFile;

internal sealed class MoveFileHandler
    : ICommandHandler<MoveFile>
{
    public async Task Handle(
        MoveFile command,
        CancellationToken cancellationToken)
    {
        var destinationPath = Path.Combine(
            command.Destination.FullName,
            command.SourceFile.Name);

        await Task.Run(() =>
        {
            command.SourceFile
                .MoveTo(destinationPath);
        }, cancellationToken);
    }
}
