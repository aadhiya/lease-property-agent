namespace LeasePropertyAgent.Application.Models.Workspace;

public class IssueWorkspaceResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string? ConditionAssessment { get; set; }
    public decimal? Confidence { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }

    public List<IssueImageWorkspaceResponse> Images { get; set; } = new();
    public List<WorkOrderWorkspaceResponse> WorkOrders { get; set; } = new();
}

public class IssueImageWorkspaceResponse
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public string? Observation { get; set; }
    public decimal? Confidence { get; set; }
}

public class WorkOrderWorkspaceResponse
{
    public Guid Id { get; set; }
    public Guid UnitId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? ReviewedAt { get; set; }
}