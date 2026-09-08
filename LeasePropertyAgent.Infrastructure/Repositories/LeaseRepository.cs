using LeasePropertyAgent.Application.Interfaces;
using LeasePropertyAgent.Domain.Entities;
using LeasePropertyAgent.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LeasePropertyAgent.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation of lease persistence.
///
/// SQLite is used for the take-home so the entire application can run
/// locally without requiring a separate database server. The repository
/// hides EF Core details from the application layer.
/// </summary>
public class LeaseRepository : ILeaseRepository
{
    private readonly LeasePropertyDbContext _dbContext;

    public LeaseRepository(LeasePropertyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Lease?> GetByIdAsync(
        Guid leaseId,
        CancellationToken cancellationToken = default)
    {
        return await _dbContext.Leases
            .Include(lease => lease.Parties)
            .Include(lease => lease.Fields)
            .Include(lease => lease.Flags)
            .Include(lease => lease.ValidationResults)
            .FirstOrDefaultAsync(
                lease => lease.Id == leaseId,
                cancellationToken);
    }

    public async Task AddAsync(
        Lease lease,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Leases.AddAsync(
            lease,
            cancellationToken);
    }

    public async Task SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }
}