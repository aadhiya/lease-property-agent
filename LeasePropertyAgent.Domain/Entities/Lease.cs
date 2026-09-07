using LeasePropertyAgent.Domain.Enums;
using LeasePropertyAgent.Domain.ValueObjects;

namespace LeasePropertyAgent.Domain.Entities;

public class Lease
{
    public Guid Id { get; set; }

    public Guid UnitId { get; set; }

    public string DocumentName { get; set; } = string.Empty;

    public string? DocumentPath { get; set; }

    public LeaseStatus Status { get; set; }

    public DateTime? CommencementDate { get; set; }

    public DateTime? ExpiryDate { get; set; }

    public int? TermMonths { get; set; }

    public decimal? MonthlyRent { get; set; }

    public decimal? AnnualRent { get; set; }

    public string? RentFrequency { get; set; }

    public string? Currency { get; set; }

    public decimal? DepositAmount { get; set; }

    public EscalationClause EscalationClause { get; set; } = new();

    public string? RenewalTerms { get; set; }

    public string? TerminationTerms { get; set; }

    public DateTime CreatedAt { get; set; }

    public Unit? Unit { get; set; }

    public List<LeaseParty> Parties { get; set; } = new();

    public List<LeaseField> Fields { get; set; } = new();

    public List<LeaseFlag> Flags { get; set; } = new();

    public List<ValidationResult> ValidationResults { get; set; } = new();
}