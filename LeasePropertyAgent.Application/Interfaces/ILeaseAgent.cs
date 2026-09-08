using LeasePropertyAgent.Application.Models;

namespace LeasePropertyAgent.Application.Interfaces;

/// <summary>
/// Coordinates lease-document understanding.
///
/// The agent is responsible for turning document content into a
/// structured lease representation with source traceability and
/// extraction-level flags. Deterministic owner-rule validation remains
/// the responsibility of ILeaseValidationService.
/// </summary>
public interface ILeaseAgent
{
    Task<LeaseExtractionResult> ExtractAsync(
        string documentPath,
        CancellationToken cancellationToken = default);
}