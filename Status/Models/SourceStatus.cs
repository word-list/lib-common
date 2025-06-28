namespace WordList.Common.Status.Models;

public record SourceStatus
{
    /// <summary>
    /// Text representation of the status.
    /// </summary>
    public string Text { get; }

    /// <summary>
    /// Used to prevent a lower priority status from overwriting a higher priority one.
    /// </summary>
    public int Priority { get; }

    private SourceStatus(string text, int priority)
    {
        Text = text;
        Priority = priority;
    }

    public static SourceStatus FromText(string text)
        => ALL.FirstOrDefault(s => s.Text.Equals(text, StringComparison.OrdinalIgnoreCase), UNKNOWN);

    public static SourceStatus[] ALL = [UNKNOWN!, CHUNKING!, QUERYING!, UPDATING!, COMPLETE!];

    public static SourceStatus UNKNOWN { get; } = new("Unknown", 0);
    public static SourceStatus CHUNKING { get; } = new("Chunking", 1);
    public static SourceStatus QUERYING { get; } = new("Querying", 2);
    public static SourceStatus UPDATING { get; } = new("Updating", 3);
    public static SourceStatus COMPLETE { get; } = new("Complete", 4);
}