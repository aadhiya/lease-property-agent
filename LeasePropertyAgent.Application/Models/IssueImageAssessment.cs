namespace LeasePropertyAgent.Application.Models;

public class IssueImageAssessment
{
    public string FileName { get; set; } = string.Empty;

    public string Observation { get; set; } = string.Empty;

    public decimal Confidence { get; set; }

    public List<string> VisibleItems { get; set; } = new();
}