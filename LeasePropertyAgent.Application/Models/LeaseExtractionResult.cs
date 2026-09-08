namespace LeasePropertyAgent.Application.Models;

/// <summary>
/// Contains the structured information produced by the lease agent.
///
/// This is the boundary between AI/document reasoning and the rest
/// of the application. The result can later be mapped into domain
/// entities, validated against owner rules, and presented for review.
/// </summary>
public class LeaseExtractionResult
{
    public string? UnitId { get; set; }

    public string? LandlordName { get; set; }

    public bool LandlordPresent { get; set; }

    public bool LandlordSigned { get; set; }

    public string? TenantName { get; set; }

    public bool TenantPresent { get; set; }

    public bool TenantSigned { get; set; }

    public DateTime? CommencementDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public int? TermMonths { get; set; }

    public decimal? MonthlyRent { get; set; }

    public decimal? AnnualRent { get; set; }

    public string? RentFrequency { get; set; }

    public string? Currency { get; set; }

    public decimal? DepositAmount { get; set; }

    public bool EscalationIsDefined { get; set; }

    public string? EscalationType { get; set; }

    public decimal? EscalationPercentage { get; set; }

    public decimal? EscalationAmount { get; set; }

    public string? EscalationFrequency { get; set; }

    public string? EscalationDescription { get; set; }

    public string? RenewalTerms { get; set; }

    public string? TerminationTerms { get; set; }

    public List<ExtractedLeaseField> Fields { get; set; } = new();

    public List<ExtractedLeaseFlag> Flags { get; set; } = new();
}