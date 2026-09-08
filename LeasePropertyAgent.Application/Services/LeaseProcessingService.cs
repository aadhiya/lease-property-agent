using LeasePropertyAgent.Application.Interfaces;
using LeasePropertyAgent.Application.Models;
using LeasePropertyAgent.Domain.Entities;
using LeasePropertyAgent.Domain.Enums;
using LeasePropertyAgent.Domain.ValueObjects;

namespace LeasePropertyAgent.Application.Services;

/// <summary>
/// Coordinates the complete lease-processing workflow:
/// extraction, catalog matching, domain mapping, validation,
/// and persistence.
///
/// The service depends only on application abstractions. Database-specific
/// behavior remains behind repository interfaces.
/// </summary>
public class LeaseProcessingService : ILeaseProcessingService
{
    private readonly ILeaseAgent _leaseAgent;
    private readonly IUnitMatchingService _unitMatchingService;
    private readonly ILeaseValidationService _validationService;
    private readonly IUnitRepository _unitRepository;
    private readonly ILeaseRepository _leaseRepository;

    public LeaseProcessingService(
        ILeaseAgent leaseAgent,
        IUnitMatchingService unitMatchingService,
        ILeaseValidationService validationService,
        IUnitRepository unitRepository,
        ILeaseRepository leaseRepository)
    {
        _leaseAgent = leaseAgent;
        _unitMatchingService = unitMatchingService;
        _validationService = validationService;
        _unitRepository = unitRepository;
        _leaseRepository = leaseRepository;
    }
private static ValidationStatus MapValidationStatus(string? status)
{
    if (string.Equals(
            status,
            "PASS",
            StringComparison.OrdinalIgnoreCase))
    {
        return ValidationStatus.Pass;
    }

    if (string.Equals(
            status,
            "FAIL",
            StringComparison.OrdinalIgnoreCase))
    {
        return ValidationStatus.Fail;
    }

    // Anything that cannot be confidently mapped is treated as
    // NotDeterminable rather than incorrectly passing validation.
    return ValidationStatus.NotDeterminable;
}

private static FlagSeverity MapFlagSeverity(string? severity)
{
    if (string.Equals(
            severity,
            "HIGH",
            StringComparison.OrdinalIgnoreCase))
    {
        return FlagSeverity.High;
    }

    if (string.Equals(
            severity,
            "MEDIUM",
            StringComparison.OrdinalIgnoreCase))
    {
        return FlagSeverity.Medium;
    }

    return FlagSeverity.Low;
}
    public async Task<LeaseProcessingResult> ProcessAsync(
        string documentPath,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Step 1: Let the lease agent understand the document and produce
        // structured fields, source evidence, and extraction flags.
        var extraction = await _leaseAgent.ExtractAsync(
            documentPath,
            cancellationToken);

        // Step 2: Resolve the extracted external unit ID against the
        // owner-provided catalog.
        var unitMatch = await _unitMatchingService.MatchUnitAsync(
            extraction.UnitId,
            cancellationToken);

        // Step 3: Map the extracted information into the domain lease.
        var lease = MapToDomainLease(
            extraction,
            documentPath);

        // Step 4: Resolve the external unit number to the internal
        // database Guid used by Lease.UnitId.
        if (unitMatch.CanLinkLease &&
            !string.IsNullOrWhiteSpace(unitMatch.UnitId))
        {
            var databaseUnit = await _unitRepository.GetByUnitNumberAsync(
                unitMatch.UnitId,
                cancellationToken);

            if (databaseUnit != null)
            {
                lease.UnitId = databaseUnit.Id;
                lease.Unit = databaseUnit;
            }
        }

        // Step 5: Apply the deterministic owner acceptance rules.
        var validation = await _validationService.ValidateAsync(
            lease,
            unitMatch,
            cancellationToken);

        // Step 6: Persist the validation results alongside the lease.
        // This creates an auditable snapshot of this validation run.
        foreach (var result in validation.Results)
        {
            lease.ValidationResults.Add(new ValidationResult
            {
                Id = Guid.NewGuid(),
                ValidationRunId = validation.ValidationRunId,
                RuleId = result.RuleId,
                Status = MapValidationStatus(result.Status),
                Reason = result.Reason,
                SourcePage = result.SourcePage,
                SourceText = result.SourceText,
                CreatedAt = DateTime.UtcNow
            });
        }

        // Step 7: Persist extracted fields and their source evidence.
        foreach (var field in extraction.Fields)
        {
            lease.Fields.Add(new LeaseField
            {
                Id = Guid.NewGuid(),
                FieldName = field.FieldName,
                ExtractedValue = field.Value,
                Confidence = field.Confidence,
                SourcePage = field.SourcePage,
                SourceText = field.SourceText,
                ReviewStatus = LeaseFieldReviewStatus.Pending
            });
        }

        // Step 8: Persist extraction flags. These remain separate from
        // deterministic validation results because they represent agent-
        // detected concerns rather than owner-rule outcomes.
        foreach (var flag in extraction.Flags)
        {
            lease.Flags.Add(new LeaseFlag
            {
                Id = Guid.NewGuid(),
                Type = flag.Type,
                Severity = MapFlagSeverity(flag.Severity),
                Message = flag.Message,
                SourcePage = flag.SourcePage,
                SourceText = flag.SourceText,
                Status = ReviewStatus.Pending
            });
        }

        // Step 9: Persist the complete aggregate only after all dependent
        // information has been attached to the domain lease.
        await _leaseRepository.AddAsync(
            lease,
            cancellationToken);

        await _leaseRepository.SaveChangesAsync(
            cancellationToken);

        return new LeaseProcessingResult
        {
            Lease = lease,
            Extraction = extraction,
            UnitMatch = unitMatch,
            Validation = validation
        };
    }

    /// <summary>
    /// Maps the agent's structured extraction into the domain lease model.
    /// The database unit relationship is intentionally resolved separately
    /// because the agent works with the external catalog unit number while
    /// the domain model uses a Guid foreign key.
    /// </summary>
    private static Lease MapToDomainLease(
        LeaseExtractionResult extraction,
        string documentPath)
    {
        var lease = new Lease
        {
            DocumentName = Path.GetFileName(documentPath),
            DocumentPath = documentPath,
            Status = LeaseStatus.PendingReview,
            CommencementDate = extraction.CommencementDate,
            ExpiryDate = extraction.ExpiryDate,
            TermMonths = extraction.TermMonths,
            MonthlyRent = extraction.MonthlyRent,
            AnnualRent = extraction.AnnualRent,
            RentFrequency = extraction.RentFrequency,
            Currency = extraction.Currency,
            DepositAmount = extraction.DepositAmount,

            EscalationClause = new EscalationClause
            {
                IsDefined = extraction.EscalationIsDefined,
                Type = extraction.EscalationType,
                Percentage = extraction.EscalationPercentage,
                Amount = extraction.EscalationAmount,
                Frequency = extraction.EscalationFrequency,
                Description = extraction.EscalationDescription
            },

            RenewalTerms = extraction.RenewalTerms,
            TerminationTerms = extraction.TerminationTerms
        };

        lease.Parties.Add(new LeaseParty
        {
            Id = Guid.NewGuid(),
            Role = PartyRole.Landlord,
            Name = extraction.LandlordName ?? string.Empty,
            IsPresent = extraction.LandlordPresent,
            IsSigned = extraction.LandlordSigned
        });

        lease.Parties.Add(new LeaseParty
        {
            Id = Guid.NewGuid(),
            Role = PartyRole.Tenant,
            Name = extraction.TenantName ?? string.Empty,
            IsPresent = extraction.TenantPresent,
            IsSigned = extraction.TenantSigned
        });

        return lease;
    }
}