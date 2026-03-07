using System.ComponentModel.DataAnnotations;

namespace SendHub.Infrastructure.Database;

public sealed record DatabaseSettings
{
    public const string SectionName = "SendHub:Database";

    /// <summary>
    /// Full path to the SQLite database file.
    /// </summary>
    [Required]
    public required string Path { get; init; }
}
