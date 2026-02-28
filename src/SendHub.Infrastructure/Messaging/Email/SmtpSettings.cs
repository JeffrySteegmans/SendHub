using System.ComponentModel.DataAnnotations;

namespace SendHub.Infrastructure.Messaging.Email;

internal sealed record SmtpSettings
{
    public const string SectionName = "SendHub:Email:Smtp";

    /// <summary>
    /// SMTP server hostname or IP address.
    /// </summary>
    [Required]
    public required string Host { get; init; }

    /// <summary>
    /// SMTP server port (e.g. 587 for STARTTLS, 465 for SSL).
    /// </summary>
    [Range(1, 65535)]
    public required int Port { get; init; }

    /// <summary>
    /// Sender email address (From header).
    /// </summary>
    [Required]
    public required string From { get; init; }

    /// <summary>
    /// Recipient email address (To header).
    /// </summary>
    [Required]
    public required string To { get; init; }

    /// <summary>
    /// SMTP username for authentication. Leave null for anonymous relay.
    /// </summary>
    public string? Username { get; init; }

    /// <summary>
    /// SMTP password for authentication.
    /// </summary>
    public string? Password { get; init; }

    /// <summary>
    /// Whether to use SSL/TLS for the SMTP connection.
    /// </summary>
    public bool EnableSsl { get; init; } = true;
}
