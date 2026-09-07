using LeasePropertyAgent.Domain.Enums;

namespace LeasePropertyAgent.Domain.Entities;

public class LeaseField
{
    public Guid Id { get; set; }

    public Guid LeaseId { get; set; }

    public string FieldName { get; set; } = string.Empty;

    public string? ExtractedValue { get; set; }

    public string? ReviewedValue { get; set; }

    public decimal? Confidence { get; set; }

    public int? SourcePage { get; set; }

    public string? SourceText { get; set; }

    public LeaseFieldReviewStatus ReviewStatus { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public Lease? Lease { get; set; }
}