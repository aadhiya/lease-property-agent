using LeasePropertyAgent.Application.Interfaces;
using LeasePropertyAgent.Domain.Entities;
using LeasePropertyAgent.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LeasePropertyAgent.Infrastructure.Repositories;

/// <summary>
/// EF Core implementation for resolving units by their business-facing
/// unit number.
///
/// UnitNumber is indexed in the database, making this lookup suitable
/// for the lease-ingestion workflow.
/// </summary>
public class UnitRepository : IUnitRepository
{
    private readonly LeasePropertyDbContext _dbContext;

    public UnitRepository(LeasePropertyDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<Unit?> GetByUnitNumberAsync(
        string unitNumber,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(unitNumber))
        {
            return null;
        }

        return await _dbContext.Units
            .FirstOrDefaultAsync(
                unit => unit.UnitNumber == unitNumber,
                cancellationToken);
    }
    public async Task<Unit?> GetByIdAsync(
    Guid unitId,
    CancellationToken cancellationToken = default)
{
    if (unitId == Guid.Empty)
    {
        return null;
    }

    return await _dbContext.Units
        .FirstOrDefaultAsync(
            unit => unit.Id == unitId,
            cancellationToken);
}
public async Task<Unit?> GetWorkspaceByIdAsync(
    Guid unitId,
    CancellationToken cancellationToken = default)
{
    if (unitId == Guid.Empty)
    {
        return null;
    }

    return await _dbContext.Units
        .Include(unit => unit.Building)
            .ThenInclude(building => building!.Property)

        .Include(unit => unit.Leases)
            .ThenInclude(lease => lease.Parties)

        .Include(unit => unit.Leases)
            .ThenInclude(lease => lease.Fields)

        .Include(unit => unit.Leases)
            .ThenInclude(lease => lease.Flags)

        .Include(unit => unit.Leases)
            .ThenInclude(lease => lease.ValidationResults)

        .Include(unit => unit.Issues)
            .ThenInclude(issue => issue.Images)

        .Include(unit => unit.Issues)
            .ThenInclude(issue => issue.WorkOrders)

        .AsSplitQuery()
        .FirstOrDefaultAsync(
            unit => unit.Id == unitId,
            cancellationToken);
}
}