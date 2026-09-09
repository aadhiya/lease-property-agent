namespace LeasePropertyAgent.Application.Models;

public class IssueProcessingRequest
{
    public Guid UnitId { get; set; }

    public List<IssueImageInput> Images { get; set; } = new();
}