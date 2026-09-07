namespace LeasePropertyAgent.Domain.Entities;

public class IssueImage
{
    public Guid Id { get; set; }

    public Guid IssueId { get; set; }

    public string FileName { get; set; } = string.Empty;

    public string FilePath { get; set; } = string.Empty;

    public Issue? Issue { get; set; }
}