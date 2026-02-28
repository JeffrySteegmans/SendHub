using Microsoft.Extensions.Logging;

namespace SendHub.Features.FileProcessing;

internal sealed class ProcessIncomingFileHandler(
    ILogger<ProcessIncomingFileHandler> logger,
    IEnumerable<IFileSender> fileSenders,
    IFileArchiver archiver,
    IProcessedFileTracker tracker) : ICommandHandler<ProcessIncomingFile>
{
    public async Task Handle(
        ProcessIncomingFile command,
        CancellationToken cancellationToken)
    {
        var file = command.File;

        // Idempotency guard
        if (await tracker.WasProcessed(file.FullName))
        {
            LogMessages
                .AlreadyProcessed(logger, file.Name);

            await archiver
                .Archive(file, command.DestinationFolder, cancellationToken);
            return;
        }

        var senders = fileSenders
            .ToList();

        // Early return if no senders configured
        if (senders.Count == 0)
        {
            LogMessages
                .NoSendersConfigured(logger, command.File.Name);
            return;
        }

        LogMessages
            .ProcessingFileWithSenders(logger, command.File.Name, senders.Count);

        // Phase 1: Send to all senders, collect failures
        var results = await Task.WhenAll(
            senders.Select(s => SendSafely(s, file, cancellationToken)));

        var failures = results
            .Where(r => r.Error != null)
            .ToList();

        if (failures.Count > 0)
        {
            LogMessages
                .ProcessingFailed(logger, failures.Count, senders.Count, file.Name);
            throw new FileSendException(file.Name, failures!);
        }

        // Phase 2: All senders succeeded — archive and track
        LogMessages
            .AllSendersSucceeded(logger, file.Name);

        await archiver
            .Archive(file, command.DestinationFolder, cancellationToken);
        await tracker
            .MarkProcessed(file.FullName);

        LogMessages
            .FileProcessed(logger, file.Name);
    }

    private async Task<(string Sender, Exception? Error)> SendSafely(
        IFileSender sender, FileInfo file, CancellationToken ct)
    {
        try
        {
            LogMessages
                .SendingViaProvider(logger, file.Name, sender.Name);
            await sender.Send(file, ct);

            LogMessages
                .SentViaSender(logger, file.Name, sender.Name);
            return (sender.Name, null);
        }
        catch (Exception ex)
        {
            LogMessages.FailedToSend(logger, file.Name, sender.Name, ex);
            return (sender.Name, ex);
        }
    }
}
