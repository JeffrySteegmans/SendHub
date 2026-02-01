using NodaTime;

namespace SendHub.Infrastructure.Tracking;

/// <summary>
/// JSON-serializable record for a single tracked file entry.
/// </summary>
internal sealed record TrackedFile(
    string FilePath,
    Instant ProcessedAt);
