using LeasePropertyAgent.Application.Models;

namespace LeasePropertyAgent.Application.Interfaces;

/// <summary>
/// Coordinates the complete lease-processing workflow.
///
/// This service deliberately orchestrates existing components instead
/// of duplicating document extraction, unit matching, or validation logic.
/// </summary>
public interface ILeaseProcessingService
{
    Task<LeaseProcessingResult> ProcessAsync(
        string documentPath,
        CancellationToken cancellationToken = default);
}