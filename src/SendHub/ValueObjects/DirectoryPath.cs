namespace SendHub.ValueObjects;

/// <summary>
/// Represents a validated directory path.
/// </summary>
public sealed record DirectoryPath
{
    private DirectoryPath(
        string path)
    {
        Value = path;
    }

    public string Value { get; }

    /// <summary>
    /// Creates a DirectoryPath from a string, validating it exists or can be created.
    /// </summary>
    public static DirectoryPath From(
        string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            throw new ArgumentException("Directory path cannot be empty", nameof(path));

        // Normalize path (handle ~, relative paths, etc.)
        var fullPath = Path.GetFullPath(path);

        // Validate it's a valid path format
        return !IsValidPath(fullPath)
            ? throw new ArgumentException($"Invalid directory path: {path}", nameof(path))
            : new DirectoryPath(fullPath);
    }

    /// <summary>
    /// Gets the DirectoryInfo for file system operations.
    /// </summary>
    public DirectoryInfo ToDirectoryInfo()
        => new(Value);

    /// <summary>
    /// Combines this directory with a relative path.
    /// </summary>
    public DirectoryPath Combine(
        string relativePath)
    {
        var combined = Path.Combine(Value, relativePath);
        return new DirectoryPath(combined);
    }

    private static bool IsValidPath(
        string path)
    {
        try
        {
            // This will throw if path contains invalid characters
            _ = Path.GetFullPath(path);
            return true;
        }
        catch
        {
            return false;
        }
    }

    public static implicit operator string(DirectoryPath path)
        => path.Value;

    public override string ToString()
        => Value;
}
