namespace LeasePropertyAgent.Application.Models;

public class IssueAssessment
{
    public string Title { get; set; } = string.Empty;

    public string Description { get; set; } = string.Empty;

    public string ConditionAssessment { get; set; } = string.Empty;

    public decimal Confidence { get; set; }

    public List<IssueImageAssessment> ImageAssessments { get; set; } = new();
}