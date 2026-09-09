using LeasePropertyAgent.Domain.Entities;

namespace LeasePropertyAgent.Application.Models;

public class IssueProcessingResult
{
    public Issue Issue { get; set; } = null!;

    public IssueAgentResult AgentResult { get; set; } = null!;
}