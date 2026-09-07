using LeasePropertyAgent.Application.Interfaces;
using LeasePropertyAgent.Application.Models;
using LeasePropertyAgent.Domain.Entities;
using LeasePropertyAgent.Domain.Enums;

namespace LeasePropertyAgent.Application.Services;

/// <summary>
/// Deterministic lease validation engine.
///
/// AI is responsible for extracting facts from the lease.
/// This service applies the owner's explicit business rules to those
/// extracted facts so that PASS/FAIL decisions remain reproducible
/// and explainable.
/// </summary>
public class LeaseValidationService : ILeaseValidationService
{
    private readonly IOwnerRulesetProvider _ownerRulesetProvider;

    public LeaseValidationService(
        IOwnerRulesetProvider ownerRulesetProvider)
    {
        _ownerRulesetProvider = ownerRulesetProvider;
    }

    public async Task<LeaseValidationReport> ValidateAsync(
        Lease lease,
        UnitMatchResult? unitMatchResult,
        CancellationToken cancellationToken = default)
    {
        var ruleset = await _ownerRulesetProvider.GetRulesetAsync();

        var report = new LeaseValidationReport();

        // R1: Deposit must be greater than or equal to monthly rent.
var r1Rule = ruleset.Rules.FirstOrDefault(r => r.Id == "R1");

if (r1Rule != null)
{
    report.Results.Add(ValidateDepositRule(lease, r1Rule));
}

// R2: An escalation clause must be defined.
var r2Rule = ruleset.Rules.FirstOrDefault(r => r.Id == "R2");

if (r2Rule != null)
{
    report.Results.Add(ValidateEscalationRule(lease, r2Rule));
}
// R3: Lease term must not exceed 36 months.
var r3Rule = ruleset.Rules.FirstOrDefault(r => r.Id == "R3");

if (r3Rule != null)
{
    report.Results.Add(ValidateTermRule(lease, r3Rule));
}
// R4: Expiry must be after commencement and the declared term
// must match the number of complete calendar months between the dates.
var r4Rule = ruleset.Rules.FirstOrDefault(r => r.Id == "R4");

if (r4Rule != null)
{
    report.Results.Add(ValidateLeaseDateRule(lease, r4Rule));
}
// R5: Landlord and tenant must both be present and signed.
var r5Rule = ruleset.Rules.FirstOrDefault(r => r.Id == "R5");

if (r5Rule != null)
{
    report.Results.Add(ValidatePartySignatureRule(lease, r5Rule));
}
// R6-R7 will be added incrementally as each rule is implemented.
foreach (var rule in ruleset.Rules.Where(
             r => r.Id != "R1" &&
                  r.Id != "R2" &&
                  r.Id != "R3" &&
                  r.Id != "R4" &&
                  r.Id != "R5"))
{
    report.Results.Add(new RuleValidationResult
    {
        RuleId = rule.Id,
        Status = "NOT_DETERMINABLE",
        Reason = "Validation logic has not yet been implemented for this rule.",
        Severity = rule.Severity
    });
}
        return report;
    }

    /// <summary>
    /// Validates R1: the security deposit must be greater than
    /// or equal to the monthly rent.
    ///
    /// Missing values cannot safely be interpreted as a failure,
    /// therefore the result is NOT_DETERMINABLE.
    /// </summary>
    private static RuleValidationResult ValidateDepositRule(
        Lease lease,
        OwnerRule rule)
    {
        if (!lease.DepositAmount.HasValue)
        {
            return new RuleValidationResult
            {
                RuleId = rule.Id,
                Status = "NOT_DETERMINABLE",
                Reason = "Deposit amount is missing from the extracted lease data.",
                Severity = rule.Severity
            };
        }

        if (!lease.MonthlyRent.HasValue)
        {
            return new RuleValidationResult
            {
                RuleId = rule.Id,
                Status = "NOT_DETERMINABLE",
                Reason = "Monthly rent is missing from the extracted lease data.",
                Severity = rule.Severity
            };
        }

        if (lease.DepositAmount.Value >= lease.MonthlyRent.Value)
        {
            return new RuleValidationResult
            {
                RuleId = rule.Id,
                Status = "PASS",
                Reason =
                    $"Deposit amount ({lease.DepositAmount.Value}) is greater than or equal to monthly rent ({lease.MonthlyRent.Value}).",
                Severity = rule.Severity
            };
        }

        return new RuleValidationResult
        {
            RuleId = rule.Id,
            Status = "FAIL",
            Reason =
                $"Deposit amount ({lease.DepositAmount.Value}) is less than monthly rent ({lease.MonthlyRent.Value}).",
            Severity = rule.Severity
        };
    }
    /// <summary>
/// Validates R2: the lease must contain a defined escalation clause.
///
/// A known absence of an escalation clause is a FAIL because the
/// owner's rules explicitly require one. We therefore do not treat
/// IsDefined == false as missing information.
/// </summary>
private static RuleValidationResult ValidateEscalationRule(
    Lease lease,
    OwnerRule rule)
{
    if (lease.EscalationClause.IsDefined)
    {
        return new RuleValidationResult
        {
            RuleId = rule.Id,
            Status = "PASS",
            Reason = "An escalation clause is defined in the lease.",
            Severity = rule.Severity
        };
    }

    return new RuleValidationResult
    {
        RuleId = rule.Id,
        Status = "FAIL",
        Reason = "The lease does not contain a defined escalation clause.",
        Severity = rule.Severity
    };
}
/// <summary>
/// Validates R3: the lease term must not exceed 36 months.
///
/// A missing term cannot be safely interpreted as either compliant
/// or non-compliant, so the result is NOT_DETERMINABLE.
/// </summary>
private static RuleValidationResult ValidateTermRule(
    Lease lease,
    OwnerRule rule)
{
    if (!lease.TermMonths.HasValue)
    {
        return new RuleValidationResult
        {
            RuleId = rule.Id,
            Status = "NOT_DETERMINABLE",
            Reason = "Lease term is missing from the extracted lease data.",
            Severity = rule.Severity
        };
    }

    if (lease.TermMonths.Value <= 36)
    {
        return new RuleValidationResult
        {
            RuleId = rule.Id,
            Status = "PASS",
            Reason =
                $"Lease term ({lease.TermMonths.Value} months) does not exceed the maximum allowed term of 36 months.",
            Severity = rule.Severity
        };
    }

    return new RuleValidationResult
    {
        RuleId = rule.Id,
        Status = "FAIL",
        Reason =
            $"Lease term ({lease.TermMonths.Value} months) exceeds the maximum allowed term of 36 months.",
        Severity = rule.Severity
    };
}
/// <summary>
/// Validates R4:
///
/// 1. Expiry date must be after commencement date.
/// 2. The declared term must match the number of complete calendar
///    months between commencement and expiry.
///
/// Missing dates or term information cannot safely be evaluated,
/// therefore those cases return NOT_DETERMINABLE.
/// </summary>
private static RuleValidationResult ValidateLeaseDateRule(
    Lease lease,
    OwnerRule rule)
{
    if (!lease.CommencementDate.HasValue)
    {
        return new RuleValidationResult
        {
            RuleId = rule.Id,
            Status = "NOT_DETERMINABLE",
            Reason = "Commencement date is missing from the extracted lease data.",
            Severity = rule.Severity
        };
    }

    if (!lease.ExpiryDate.HasValue)
    {
        return new RuleValidationResult
        {
            RuleId = rule.Id,
            Status = "NOT_DETERMINABLE",
            Reason = "Expiry date is missing from the extracted lease data.",
            Severity = rule.Severity
        };
    }

    if (!lease.TermMonths.HasValue)
    {
        return new RuleValidationResult
        {
            RuleId = rule.Id,
            Status = "NOT_DETERMINABLE",
            Reason = "Lease term is missing from the extracted lease data.",
            Severity = rule.Severity
        };
    }

    var commencementDate = lease.CommencementDate.Value;
    var expiryDate = lease.ExpiryDate.Value;

    if (expiryDate <= commencementDate)
    {
        return new RuleValidationResult
        {
            RuleId = rule.Id,
            Status = "FAIL",
            Reason =
                $"Expiry date ({expiryDate:yyyy-MM-dd}) must be after commencement date ({commencementDate:yyyy-MM-dd}).",
            Severity = rule.Severity
        };
    }

    var calculatedMonths = CalculateCompleteMonthsBetween(
        commencementDate,
        expiryDate);

    if (lease.TermMonths.Value != calculatedMonths)
    {
        return new RuleValidationResult
        {
            RuleId = rule.Id,
            Status = "FAIL",
            Reason =
                $"Declared lease term ({lease.TermMonths.Value} months) does not match the calculated term of {calculatedMonths} months between the commencement and expiry dates.",
            Severity = rule.Severity
        };
    }

    return new RuleValidationResult
    {
        RuleId = rule.Id,
        Status = "PASS",
        Reason =
            $"Expiry date is after commencement date and the declared term ({lease.TermMonths.Value} months) matches the calculated date difference.",
        Severity = rule.Severity
    };
}

/// <summary>
/// Calculates complete calendar months between two dates.
///
/// If the expiry day occurs before the commencement day within the
/// final month, that partial month is not counted as a complete month.
/// </summary>
private static int CalculateCompleteMonthsBetween(
    DateTime commencementDate,
    DateTime expiryDate)
{
    var months =
        ((expiryDate.Year - commencementDate.Year) * 12)
        + expiryDate.Month
        - commencementDate.Month;

    if (expiryDate.Day < commencementDate.Day)
    {
        months--;
    }

    return months;
}
/// <summary>
/// Validates R5:
///
/// - A landlord must be present.
/// - A tenant must be present.
/// - The landlord must be signed.
/// - The tenant must be signed.
///
/// Missing party information results in NOT_DETERMINABLE because
/// the system cannot safely conclude that the requirement has failed.
/// If both parties are present but either signature is missing,
/// the rule definitively fails.
/// </summary>
private static RuleValidationResult ValidatePartySignatureRule(
    Lease lease,
    OwnerRule rule)
{
    var landlord = lease.Parties
        .FirstOrDefault(p => p.Role == PartyRole.Landlord);

    var tenant = lease.Parties
        .FirstOrDefault(p => p.Role == PartyRole.Tenant);

    if (landlord == null || !landlord.IsPresent)
    {
        return new RuleValidationResult
        {
            RuleId = rule.Id,
            Status = "NOT_DETERMINABLE",
            Reason = "Landlord information is missing or could not be established from the lease.",
            Severity = rule.Severity
        };
    }

    if (tenant == null || !tenant.IsPresent)
    {
        return new RuleValidationResult
        {
            RuleId = rule.Id,
            Status = "NOT_DETERMINABLE",
            Reason = "Tenant information is missing or could not be established from the lease.",
            Severity = rule.Severity
        };
    }

    if (!landlord.IsSigned && !tenant.IsSigned)
    {
        return new RuleValidationResult
        {
            RuleId = rule.Id,
            Status = "FAIL",
            Reason = "Both landlord and tenant are present, but neither party has a recorded signature.",
            Severity = rule.Severity
        };
    }

    if (!landlord.IsSigned)
    {
        return new RuleValidationResult
        {
            RuleId = rule.Id,
            Status = "FAIL",
            Reason = "Landlord is present but the landlord signature is missing.",
            Severity = rule.Severity
        };
    }

    if (!tenant.IsSigned)
    {
        return new RuleValidationResult
        {
            RuleId = rule.Id,
            Status = "FAIL",
            Reason = "Tenant is present but the tenant signature is missing.",
            Severity = rule.Severity
        };
    }

    return new RuleValidationResult
    {
        RuleId = rule.Id,
        Status = "PASS",
        Reason = "Both landlord and tenant are present and both parties have recorded signatures.",
        Severity = rule.Severity
    };
}
}