using LeasePropertyAgent.Domain.Entities;

namespace LeasePropertyAgent.Application.Models;

/// <summary>
/// Represents the complete result of processing one lease document.
///
/// The result keeps extraction, unit matching, and validation together
/// so the API can later present one auditable lease-processing response.
/// </summary>
public class LeaseProcessingResult
{
    public Lease Lease { get; set; } = null!;

    public LeaseExtractionResult Extraction { get; set; } = null!;

    public UnitMatchResult UnitMatch { get; set; } = null!;

    public LeaseValidationReport Validation { get; set; } = null!;
}