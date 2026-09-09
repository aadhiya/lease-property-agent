using LeasePropertyAgent.Domain.Enums;

namespace LeasePropertyAgent.Application.Models.Workspace;

public class LeaseWorkspaceResponse
{
    public Guid Id { get; set; }
    public string DocumentName { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;

    public DateTime? CommencementDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int? TermMonths { get; set; }

    public decimal? MonthlyRent { get; set; }
    public decimal? AnnualRent { get; set; }
    public string? RentFrequency { get; set; }
    public string? Currency { get; set; }
    public decimal? DepositAmount { get; set; }

    public string? EscalationClause { get; set; }
    public string? RenewalTerms { get; set; }
    public string? TerminationTerms { get; set; }

    public List<LeasePartyWorkspaceResponse> Parties { get; set; } = new();
    public List<LeaseFieldWorkspaceResponse> Fields { get; set; } = new();
    public List<LeaseFlagWorkspaceResponse> Flags { get; set; } = new();
    public List<ValidationWorkspaceResponse> ValidationResults { get; set; } = new();
}

public class LeasePartyWorkspaceResponse
{
    public Guid Id { get; set; }
    public string Role { get; set; } = string.Empty;
    public string? Name { get; set; }
    public bool IsPresent { get; set; }
    public bool IsSigned { get; set; }
}

public class LeaseFieldWorkspaceResponse
{
    public Guid Id { get; set; }
    public string FieldName { get; set; } = string.Empty;
    public string? ExtractedValue { get; set; }
    public string? ReviewedValue { get; set; }
    public decimal? Confidence { get; set; }

    public int? SourcePage { get; set; }
    public string? SourceText { get; set; }

    public string ReviewStatus { get; set; } = string.Empty;
}

public class LeaseFlagWorkspaceResponse
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Severity { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;

    public int? SourcePage { get; set; }
    public string? SourceText { get; set; }

    public string Status { get; set; } = string.Empty;
}

public class ValidationWorkspaceResponse
{
    public Guid Id { get; set; }
    public Guid ValidationRunId { get; set; }
    public string RuleId { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Reason { get; set; } = string.Empty;

    public int? SourcePage { get; set; }
    public string? SourceText { get; set; }

    public DateTime CreatedAt { get; set; }
}