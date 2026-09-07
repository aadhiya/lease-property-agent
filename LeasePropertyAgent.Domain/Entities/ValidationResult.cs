using LeasePropertyAgent.Domain.Enums;

namespace LeasePropertyAgent.Domain.Entities;

public class ValidationResult
{
    public Guid Id { get; set; }

    // Groups all R1-R7 results produced by a single validation execution.
    // This allows us to re-run validation after a human edits an extracted field
    // without losing the ability to distinguish different validation runs.
    public Guid ValidationRunId { get; set; }

    public Guid LeaseId { get; set; }

    public string RuleId { get; set; } = string.Empty;

    public ValidationStatus Status { get; set; }

    public string Reason { get; set; } = string.Empty;

    public int? SourcePage { get; set; }

    public string? SourceText { get; set; }

    public DateTime CreatedAt { get; set; }

    public Lease? Lease { get; set; }
}