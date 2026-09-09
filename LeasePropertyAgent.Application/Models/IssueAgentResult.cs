namespace LeasePropertyAgent.Application.Models;

public class IssueAgentResult
{
    public IssueAssessment Assessment { get; set; } = new();

    public WorkOrderDraft WorkOrder { get; set; } = new();
}