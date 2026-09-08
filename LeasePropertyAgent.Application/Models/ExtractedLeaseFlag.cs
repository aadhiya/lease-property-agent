namespace LeasePropertyAgent.Application.Models;

/// <summary>
/// Represents an issue identified during lease extraction.
///
/// These are extraction/data-quality observations such as missing,
/// contradictory, or suspicious information. They are intentionally
/// separate from owner acceptance rules R1-R7.
/// </summary>
public class ExtractedLeaseFlag
{
    public string Type { get; set; } = string.Empty;

    public string Severity { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;

    public int? SourcePage { get; set; }

    public string? SourceText { get; set; }
}