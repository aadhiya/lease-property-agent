using LeasePropertyAgent.Domain.Entities;

namespace LeasePropertyAgent.Application.Interfaces;

/// <summary>
/// Provides database access to property units.
///
/// The lease agent extracts the business-facing unit number, such as
/// MC-B-1204. This repository resolves that value to the internal Unit
/// entity whose Guid is used by Lease.UnitId.
/// </summary>
public interface IUnitRepository
{
    Task<Unit?> GetByUnitNumberAsync(
        string unitNumber,
        CancellationToken cancellationToken = default);
}