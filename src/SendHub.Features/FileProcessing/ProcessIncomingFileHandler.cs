using Microsoft.Extensions.Logging;

namespace SendHub.Features.FileProcessing;

internal sealed class ProcessIncomingFileHandler(
    ILogger<ProcessIncomingFileHandler> logger) : ICommandHandler<ProcessIncomingFile>
{
    public Task Handle(
        ProcessIncomingFile command,
        CancellationToken cancellationToken)
    {
        LogMessages
            .ProcessingFile(logger, command.File.Name);

        return Task.CompletedTask;
    }
}
