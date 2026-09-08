using LeasePropertyAgent.Application.Interfaces;
using LeasePropertyAgent.Application.Models;
using LeasePropertyAgent.Domain.Entities;
using LeasePropertyAgent.Domain.Enums;
using LeasePropertyAgent.Domain.ValueObjects;
namespace LeasePropertyAgent.Application.Services;

/// <summary>
/// Coordinates lease extraction, unit matching, domain mapping,
/// and deterministic owner-rule validation.
///
/// Persistence is intentionally not handled here yet. Keeping this
/// first version persistence-free makes the orchestration independently
/// testable before we introduce database concerns.
/// </summary>
public class LeaseProcessingService : ILeaseProcessingService
{
    private readonly ILeaseAgent _leaseAgent;
   private readonly IUnitMatchingService _unitMatchingService;
    private readonly ILeaseValidationService _validationService;

    public LeaseProcessingService(
        ILeaseAgent leaseAgent,
        IUnitMatchingService unitMatchingService,
        ILeaseValidationService validationService)
    {
        _leaseAgent = leaseAgent;
        _unitMatchingService = unitMatchingService;
        _validationService = validationService;
    }

    public async Task<LeaseProcessingResult> ProcessAsync(
        string documentPath,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        // Step 1: Let the lease agent understand the document.
        var extraction = await _leaseAgent.ExtractAsync(
            documentPath,
            cancellationToken);

        // Step 2: Resolve the extracted external unit ID against
        // the owner-provided unit catalog.
        var unitMatch = await _unitMatchingService.MatchUnitAsync(
            extraction.UnitId,
            cancellationToken);

        // Step 3: Create an in-memory domain lease from the extracted
        // information. Persistence will be introduced separately.
        var lease = MapToDomainLease(extraction);

        // Step 4: Apply the deterministic owner acceptance rules.
        var validation = await _validationService.ValidateAsync(
            lease,
            unitMatch,
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
    /// Maps the agent's structured output into the domain lease model.
    ///
    /// The external catalog UnitId cannot be stored directly because
    /// the domain Lease uses the internal database Unit Guid as its FK.
    /// Unit resolution will therefore be completed once persistence is
    /// introduced.
    /// </summary>
    private static Lease MapToDomainLease(
        LeaseExtractionResult extraction)
    {
        var lease = new Lease
        {
            DocumentName = "Processed Lease",
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
            Role = PartyRole.Landlord,
            Name = extraction.LandlordName ?? string.Empty,
            IsPresent = extraction.LandlordPresent,
            IsSigned = extraction.LandlordSigned
        });

        lease.Parties.Add(new LeaseParty
        {
            Role = PartyRole.Tenant,
            Name = extraction.TenantName ?? string.Empty,
            IsPresent = extraction.TenantPresent,
            IsSigned = extraction.TenantSigned
        });

        return lease;
    }
}