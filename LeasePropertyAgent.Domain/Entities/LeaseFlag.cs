using LeasePropertyAgent.Domain.Enums;

namespace LeasePropertyAgent.Domain.Entities;

public class LeaseFlag
{
    public Guid Id { get; set; }

    public Guid LeaseId { get; set; }

    public string Type { get; set; } = string.Empty;

    public FlagSeverity Severity { get; set; }

    public string Message { get; set; } = string.Empty;

    public int? SourcePage { get; set; }

    public string? SourceText { get; set; }

    public ReviewStatus Status { get; set; }

    public Lease? Lease { get; set; }
}