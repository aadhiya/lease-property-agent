namespace LeasePropertyAgent.Application.Models;

public class IssueProcessingResponse
{
    public Guid IssueId { get; set; }
    public Guid UnitId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string ConditionAssessment { get; set; } = string.Empty;
    public decimal? Confidence { get; set; }

    public string Status { get; set; } = string.Empty;

    public List<IssueImageResponse> Images { get; set; } = new();

    public WorkOrderResponse? WorkOrder { get; set; }
}

public class IssueImageResponse
{
    public Guid Id { get; set; }

    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;

    public string? Observation { get; set; }
    public decimal? Confidence { get; set; }
}

public class WorkOrderResponse
{
    public Guid Id { get; set; }

    public Guid UnitId { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;
}