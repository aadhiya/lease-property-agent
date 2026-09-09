using LeasePropertyAgent.Domain.Entities;

namespace LeasePropertyAgent.Application.Interfaces;

/// <summary>
/// Provides database access to property units.
/// 
/// The repository supports both business-facing unit-number resolution
/// for lease ingestion and internal identity resolution for issue processing.
/// </summary>
public interface IUnitRepository
{
    Task<Unit?> GetByUnitNumberAsync(
        string unitNumber,
        CancellationToken cancellationToken = default);

    Task<Unit?> GetByIdAsync(
        Guid unitId,
        CancellationToken cancellationToken = default);
}