using LeasePropertyAgent.Application.Interfaces;
using LeasePropertyAgent.Application.Models;
using LeasePropertyAgent.Application.Services;
using LeasePropertyAgent.Domain.Entities;
using Xunit;

namespace LeasePropertyAgent.Tests.Services;

public class LeaseValidationServiceTests
{
    [Fact]
    public async Task R1_ShouldPass_WhenDepositIsEqualToMonthlyRent()
    {
        var lease = CreateLease(
            monthlyRent: 5000m,
            deposit: 5000m);

        var service = CreateService();

        var result = await service.ValidateAsync(lease, null);

        var r1 = result.Results.Single(r => r.RuleId == "R1");

        Assert.Equal("PASS", r1.Status);
    }

    [Fact]
    public async Task R1_ShouldPass_WhenDepositIsGreaterThanMonthlyRent()
    {
        var lease = CreateLease(
            monthlyRent: 5000m,
            deposit: 7500m);

        var service = CreateService();

        var result = await service.ValidateAsync(lease, null);

        var r1 = result.Results.Single(r => r.RuleId == "R1");

        Assert.Equal("PASS", r1.Status);
    }

    [Fact]
    public async Task R1_ShouldFail_WhenDepositIsLessThanMonthlyRent()
    {
        var lease = CreateLease(
            monthlyRent: 5000m,
            deposit: 3000m);

        var service = CreateService();

        var result = await service.ValidateAsync(lease, null);

        var r1 = result.Results.Single(r => r.RuleId == "R1");

        Assert.Equal("FAIL", r1.Status);
    }

    [Fact]
    public async Task R1_ShouldBeNotDetermined_WhenDepositIsMissing()
    {
        var lease = CreateLease(
            monthlyRent: 5000m,
            deposit: null);

        var service = CreateService();

        var result = await service.ValidateAsync(lease, null);

        var r1 = result.Results.Single(r => r.RuleId == "R1");

        Assert.Equal("NOT_DETERMINABLE", r1.Status);
    }

    [Fact]
    public async Task R1_ShouldBeNotDetermined_WhenMonthlyRentIsMissing()
    {
        var lease = CreateLease(
            monthlyRent: null,
            deposit: 5000m);

        var service = CreateService();

        var result = await service.ValidateAsync(lease, null);

        var r1 = result.Results.Single(r => r.RuleId == "R1");

        Assert.Equal("NOT_DETERMINABLE", r1.Status);
    }
[Fact]
public async Task R2_ShouldPass_WhenEscalationClauseIsDefined()
{
    var lease = CreateLease(
        monthlyRent: 5000m,
        deposit: 5000m);

    lease.EscalationClause.IsDefined = true;

    var service = CreateService();

    var result = await service.ValidateAsync(lease, null);

    var r2 = result.Results.Single(r => r.RuleId == "R2");

    Assert.Equal("PASS", r2.Status);
    Assert.Equal("medium", r2.Severity);
}
[Fact]
public async Task R2_ShouldFail_WhenEscalationClauseIsNotDefined()
{
    var lease = CreateLease(
        monthlyRent: 5000m,
        deposit: 5000m);

    lease.EscalationClause.IsDefined = false;

    var service = CreateService();

    var result = await service.ValidateAsync(lease, null);

    var r2 = result.Results.Single(r => r.RuleId == "R2");

    Assert.Equal("FAIL", r2.Status);
    Assert.Equal("medium", r2.Severity);
}
[Fact]
public async Task R3_ShouldPass_WhenTermIsLessThan36Months()
{
    var lease = CreateLease(
        monthlyRent: 5000m,
        deposit: 5000m);

    lease.TermMonths = 24;

    var service = CreateService();

    var result = await service.ValidateAsync(lease, null);

    var r3 = result.Results.Single(r => r.RuleId == "R3");

    Assert.Equal("PASS", r3.Status);
    Assert.Equal("medium", r3.Severity);
}
[Fact]
public async Task R3_ShouldPass_WhenTermIsExactly36Months()
{
    var lease = CreateLease(
        monthlyRent: 5000m,
        deposit: 5000m);

    lease.TermMonths = 36;

    var service = CreateService();

    var result = await service.ValidateAsync(lease, null);

    var r3 = result.Results.Single(r => r.RuleId == "R3");

    Assert.Equal("PASS", r3.Status);
}
[Fact]
public async Task R3_ShouldFail_WhenTermExceeds36Months()
{
    var lease = CreateLease(
        monthlyRent: 5000m,
        deposit: 5000m);

    lease.TermMonths = 37;

    var service = CreateService();

    var result = await service.ValidateAsync(lease, null);

    var r3 = result.Results.Single(r => r.RuleId == "R3");

    Assert.Equal("FAIL", r3.Status);
}
[Fact]
public async Task R3_ShouldBeNotDetermined_WhenTermIsMissing()
{
    var lease = CreateLease(
        monthlyRent: 5000m,
        deposit: 5000m);

    lease.TermMonths = null;

    var service = CreateService();

    var result = await service.ValidateAsync(lease, null);

    var r3 = result.Results.Single(r => r.RuleId == "R3");

    Assert.Equal("NOT_DETERMINABLE", r3.Status);
}
    private static Lease CreateLease(
        decimal? monthlyRent,
        decimal? deposit)
    {
        return new Lease
        {
            Id = Guid.NewGuid(),
            MonthlyRent = monthlyRent,
            DepositAmount = deposit
        };
    }

    private static ILeaseValidationService CreateService()
    {
        var rulesetProvider = new TestOwnerRulesetProvider();

        return new LeaseValidationService(rulesetProvider);
    }

    private class TestOwnerRulesetProvider : IOwnerRulesetProvider
    {
        public Task<OwnerRuleset> GetRulesetAsync(
    CancellationToken cancellationToken = default)
{
    return Task.FromResult(new OwnerRuleset
    {
        Rules = new List<OwnerRule>
        {
            new OwnerRule
            {
                Id = "R1",
                Description = "Deposit amount must be greater than or equal to monthly rent.",
                Check = "deposit_amount >= monthly_rent",
                Severity = "high"
            },
            new OwnerRule
            {
                Id = "R2",
                Description = "Escalation clause must be defined.",
                Check = "escalation_clause.is_defined == true",
                Severity = "medium"
            },
            new OwnerRule
            {
                Id = "R3",
                Description = "Lease term must not exceed 36 months.",
                Check = "term_months <= 36",
                Severity = "medium"
            },
            new OwnerRule
            {
                Id = "R4",
                Description = "Expiry must be after commencement and term must match.",
                Check = "expiry_date > commencement_date AND term_months == months_between(commencement_date, expiry_date)",
                Severity = "high"
            },
            new OwnerRule
            {
                Id = "R5",
                Description = "Landlord and tenant must be present and signed.",
                Check = "landlord.present AND tenant.present AND landlord.signed AND tenant.signed",
                Severity = "high"
            },
            new OwnerRule
            {
                Id = "R6",
                Description = "Annual rent must equal monthly rent multiplied by 12.",
                Check = "annual_rent == monthly_rent * 12",
                Severity = "low"
            },
            new OwnerRule
            {
                Id = "R7",
                Description = "Unit must exist and be available.",
                Check = "unit_id exists in units.json AND unit.status == 'available'",
                Severity = "high"
            }
        }
    });
}
    }
}