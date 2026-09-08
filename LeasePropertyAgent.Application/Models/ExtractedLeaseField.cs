namespace LeasePropertyAgent.Application.Models;

/// <summary>
/// Represents one field extracted by the lease agent before it is
/// persisted as a domain LeaseField.
///
/// SourcePage and SourceText provide the traceability required by the
/// assessment, while Confidence allows the UI to surface uncertain
/// extraction results for human review.
/// </summary>
public class ExtractedLeaseField
{
    public string FieldName { get; set; } = string.Empty;

    public string? Value { get; set; }

    public decimal? Confidence { get; set; }

    public int? SourcePage { get; set; }

    public string? SourceText { get; set; }
}