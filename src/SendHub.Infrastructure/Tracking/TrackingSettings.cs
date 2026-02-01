using System.ComponentModel.DataAnnotations;

namespace SendHub.Infrastructure.Tracking;

public sealed record TrackingSettings
{
    public const string SectionName = "SendHub:Tracking";

    /// <summary>
    /// Full path to the JSON file that persists processed file records.
    /// </summary>
    [Required]
    public required string FilePath { get; init; }
}
