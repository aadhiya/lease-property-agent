using LeasePropertyAgent.Domain.Entities;

namespace LeasePropertyAgent.Application.Interfaces;

public interface IIssueRepository
{
    Task AddAsync(
        Issue issue,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}