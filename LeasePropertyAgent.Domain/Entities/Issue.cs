using LeasePropertyAgent.Domain.Enums;
namespace LeasePropertyAgent.Domain.Entities;

public class Issue
{
    public Guid Id { get; set; }

    public Guid UnitId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string? ConditionAssessment { get; set; }

    public decimal? Confidence { get; set; }

    public DateTime CreatedAt { get; set; }

    public ReviewStatus Status { get; set; }

    public Unit? Unit { get; set; }

    public List<IssueImage> Images { get; set; } = new();

    public List<WorkOrder> WorkOrders { get; set; } = new();
}
