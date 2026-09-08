using LeasePropertyAgent.Domain.Entities;

namespace LeasePropertyAgent.Application.Interfaces;

/// <summary>
/// Provides persistence operations for leases.
///
/// Keeping persistence behind an application interface allows the lease
/// processing workflow to remain independent from EF Core and SQLite.
/// </summary>
public interface ILeaseRepository
{
    Task<Lease?> GetByIdAsync(
        Guid leaseId,
        CancellationToken cancellationToken = default);

    Task AddAsync(
        Lease lease,
        CancellationToken cancellationToken = default);

    Task SaveChangesAsync(
        CancellationToken cancellationToken = default);
}