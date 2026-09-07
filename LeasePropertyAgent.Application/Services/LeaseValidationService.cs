using LeasePropertyAgent.Application.Interfaces;
using LeasePropertyAgent.Application.Models;
using LeasePropertyAgent.Domain.Entities;

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

        // R1 is evaluated directly from the extracted lease values.
        // The owner ruleset supplies the rule metadata such as severity.
        var r1Rule = ruleset.Rules.FirstOrDefault(r => r.Id == "R1");

        if (r1Rule != null)
        {
            report.Results.Add(ValidateDepositRule(lease, r1Rule));
        }

        // R2-R7 will be added incrementally as each rule is implemented.
        foreach (var rule in ruleset.Rules.Where(r => r.Id != "R1"))
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
}