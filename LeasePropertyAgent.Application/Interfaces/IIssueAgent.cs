using LeasePropertyAgent.Application.Models;

namespace LeasePropertyAgent.Application.Interfaces;

public interface IIssueAgent
{
    Task<IssueAgentResult> AssessAsync(
        IReadOnlyList<IssueImageInput> images,
        CancellationToken cancellationToken = default);
}