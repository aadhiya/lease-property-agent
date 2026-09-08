using LeasePropertyAgent.Application.Models;

namespace LeasePropertyAgent.Application.Interfaces;

/// <summary>
/// Resolves an externally extracted unit identifier against the
/// owner-provided property catalog.
/// </summary>
public interface IUnitMatchingService
{
    Task<UnitMatchResult> MatchUnitAsync(
        string? unitId,
        CancellationToken cancellationToken = default);
}