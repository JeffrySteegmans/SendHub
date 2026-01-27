using Microsoft.Extensions.Logging;

namespace SendHub.Features.FileProcessing;

internal sealed class ProcessIncomingFileHandler(
    ILogger<ProcessIncomingFileHandler> logger) : ICommandHandler<ProcessIncomingFile>
{
    public Task Handle(
        ProcessIncomingFile command,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Processing file {file}", command.File);

        return Task.CompletedTask;
    }
}
