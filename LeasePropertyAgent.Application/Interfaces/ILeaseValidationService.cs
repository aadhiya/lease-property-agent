using LeasePropertyAgent.Application.Models;
using LeasePropertyAgent.Domain.Entities;

namespace LeasePropertyAgent.Application.Interfaces;

/// <summary>
/// Evaluates an extracted lease against the owner's deterministic
/// acceptance rules and returns an auditable validation report.
/// </summary>
public interface ILeaseValidationService
{
    Task<LeaseValidationReport> ValidateAsync(
        Lease lease,
        UnitMatchResult? unitMatchResult,
        CancellationToken cancellationToken = default);
}