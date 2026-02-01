namespace SendHub;

/// <summary>
/// Tracks which files have been processed to ensure idempotency.
/// </summary>
public interface IProcessedFileTracker
{
    /// <summary>
    /// Checks if a file has already been processed.
    /// </summary>
    Task<bool> WasProcessed(string filePath);

    /// <summary>
    /// Marks a file as successfully processed.
    /// </summary>
    Task MarkProcessed(string filePath);
}
