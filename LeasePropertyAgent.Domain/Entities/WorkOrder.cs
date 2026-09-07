using LeasePropertyAgent.Domain.Enums;

namespace LeasePropertyAgent.Domain.Entities;

public class WorkOrder
{
    public Guid Id { get; set; }

    public Guid IssueId { get; set; }

    public Guid UnitId { get; set; }

    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public WorkOrderStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public Issue? Issue { get; set; }

    public Unit? Unit { get; set; }
}