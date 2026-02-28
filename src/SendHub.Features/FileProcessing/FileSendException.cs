namespace SendHub.Features.FileProcessing;

public sealed class FileSendException
    : Exception
{
    //TODO: Has to be refactored to include failure details
    public FileSendException(
        string fileName,
        IEnumerable<(string, Exception)> failures)
        : base($"Failed to send file '{fileName}' via {failures.Count()} sender(s).")
    {

    }
}
