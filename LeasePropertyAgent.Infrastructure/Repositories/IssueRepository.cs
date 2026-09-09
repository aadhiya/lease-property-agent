using LeasePropertyAgent.Application.Interfaces;
using LeasePropertyAgent.Domain.Entities;
using LeasePropertyAgent.Infrastructure.Data;

namespace LeasePropertyAgent.Infrastructure.Repositories;

public class IssueRepository : IIssueRepository
{
    private readonly LeasePropertyDbContext _dbContext;

    public IssueRepository(LeasePropertyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task AddAsync(
        Issue issue,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Issues.AddAsync(issue, cancellationToken);
    }

    public Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return _dbContext.SaveChangesAsync(cancellationToken);
    }
}