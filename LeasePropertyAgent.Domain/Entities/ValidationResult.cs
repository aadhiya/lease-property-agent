using LeasePropertyAgent.Domain.Enums;

namespace LeasePropertyAgent.Domain.Entities;

public class ValidationResult
{
    public Guid Id { get; set; }

    public Guid LeaseId { get; set; }

    public string RuleId { get; set; } = string.Empty;

    public ValidationStatus Status { get; set; }

    public string Reason { get; set; } = string.Empty;

    public int? SourcePage { get; set; }

    public string? SourceText { get; set; }

    public DateTime CreatedAt { get; set; }

    public Lease? Lease { get; set; }
}