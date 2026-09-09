using LeasePropertyAgent.Application.Models;

namespace LeasePropertyAgent.Application.Interfaces;

public interface IIssueProcessingService
{
    Task<IssueProcessingResult> ProcessAsync(
        IssueProcessingRequest request,
        CancellationToken cancellationToken = default);
}