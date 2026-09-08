using LeasePropertyAgent.Application.Interfaces;
using LeasePropertyAgent.Application.Models;
using LeasePropertyAgent.Application.Services;
using LeasePropertyAgent.Domain.Entities;
using LeasePropertyAgent.Domain.Enums;
using LeasePropertyAgent.Domain.ValueObjects;

namespace LeasePropertyAgent.Tests.Services;

public class LeaseProcessingServiceTests
{
    [Fact]
    public async Task ProcessAsync_ShouldOrchestrateExtractionMatchingAndValidation()
    {
        // Arrange
        var extraction = CreateExtractionResult();

        var leaseAgent = new FakeLeaseAgent(extraction);

        var unitMatch = new UnitMatchResult
        {
            Exists = true,
            IsAvailable = true,
            CanLinkLease = true,
            UnitId = "MC-B-1204",
            UnitLabel = "Apartment 1204",
            PropertyName = "Marina Crest Residences",
            BuildingName = "Tower B",
            CurrentStatus = "available",
            Reason = "Unit exists and is available."
        };

        var unitMatchingService =
            new FakeUnitMatchingService(unitMatch);

        var validationReport = new LeaseValidationReport();

        validationReport.Results.Add(
            new RuleValidationResult
            {
                RuleId = "R1",
                Status = "PASS",
                Severity = "high",
                Reason = "Deposit is greater than or equal to monthly rent."
            });

        var validationService =
            new FakeLeaseValidationService(validationReport);

        var service = new LeaseProcessingService(
            leaseAgent,
            unitMatchingService,
            validationService);

        // Act
        var result = await service.ProcessAsync(
            "sample-lease.txt");

        // Assert
        Assert.NotNull(result);
        Assert.NotNull(result.Extraction);
        Assert.NotNull(result.UnitMatch);
        Assert.NotNull(result.Lease);
        Assert.NotNull(result.Validation);

        Assert.Equal(
            "MC-B-1204",
            result.Extraction.UnitId);

        Assert.True(
            result.UnitMatch.CanLinkLease);

        Assert.Equal(
            LeaseStatus.PendingReview,
            result.Lease.Status);

        Assert.Equal(
            12000m,
            result.Lease.MonthlyRent);

        Assert.Equal(
            144000m,
            result.Lease.AnnualRent);

        Assert.Single(
            result.Validation.Results);

        Assert.Equal(
            "R1",
            result.Validation.Results[0].RuleId);

        Assert.True(
            leaseAgent.WasCalled);

        Assert.True(
            unitMatchingService.WasCalled);

        Assert.True(
            validationService.WasCalled);
    }

    [Fact]
    public async Task ProcessAsync_ShouldPassMissingUnitIdToUnitMatchingService()
    {
        // Arrange
        var extraction = CreateExtractionResult();
        extraction.UnitId = null;

        var leaseAgent = new FakeLeaseAgent(extraction);

        var expectedMatch = new UnitMatchResult
        {
            Exists = false,
            IsAvailable = false,
            CanLinkLease = false,
            Reason = "Unit ID was not provided."
        };

        var unitMatchingService =
            new FakeUnitMatchingService(expectedMatch);

        var validationReport = new LeaseValidationReport();

        validationReport.Results.Add(
            new RuleValidationResult
            {
                RuleId = "R7",
                Status = "NOT_DETERMINABLE",
                Severity = "high",
                Reason = "Unit could not be matched."
            });

        var validationService =
            new FakeLeaseValidationService(validationReport);

        var service = new LeaseProcessingService(
            leaseAgent,
            unitMatchingService,
            validationService);

        // Act
        var result = await service.ProcessAsync(
            "sample-lease.txt");

        // Assert
        Assert.Null(result.Extraction.UnitId);

        Assert.False(
            result.UnitMatch.CanLinkLease);

        Assert.Equal(
            "R7",
            result.Validation.Results[0].RuleId);

        Assert.Equal(
            "NOT_DETERMINABLE",
            result.Validation.Results[0].Status);

        Assert.True(
            unitMatchingService.WasCalled);

        Assert.Null(
            unitMatchingService.ReceivedUnitId);
    }

    [Fact]
    public async Task ProcessAsync_ShouldPreserveExtractionFlags()
    {
        // Arrange
        var extraction = CreateExtractionResult();

        extraction.Flags.Add(
            new ExtractedLeaseFlag
            {
                Type = "SuspiciousValue",
                Severity = "High",
                Message = "Monthly rent appears suspicious."
            });

        var leaseAgent = new FakeLeaseAgent(extraction);

        var unitMatchingService =
            new FakeUnitMatchingService(
                new UnitMatchResult
                {
                    Exists = true,
                    IsAvailable = true,
                    CanLinkLease = true,
                    UnitId = "MC-B-1204"
                });

        var validationService =
            new FakeLeaseValidationService(
                new LeaseValidationReport());

        var service = new LeaseProcessingService(
            leaseAgent,
            unitMatchingService,
            validationService);

        // Act
        var result = await service.ProcessAsync(
            "sample-lease.txt");

        // Assert
        Assert.Single(result.Extraction.Flags);

        Assert.Equal(
            "SuspiciousValue",
            result.Extraction.Flags[0].Type);
    }

    private static LeaseExtractionResult CreateExtractionResult()
    {
        return new LeaseExtractionResult
        {
            UnitId = "MC-B-1204",
            LandlordName = "Marina Crest Holdings W.L.L.",
            LandlordPresent = true,
            LandlordSigned = true,
            TenantName = "Ahmed Hassan",
            TenantPresent = true,
            TenantSigned = true,
            CommencementDate = new DateTime(2026, 1, 1),
            ExpiryDate = new DateTime(2027, 12, 31),
            TermMonths = 24,
            MonthlyRent = 12000m,
            AnnualRent = 144000m,
            RentFrequency = "Monthly",
            Currency = "QAR",
            DepositAmount = 12000m,
            EscalationIsDefined = true,
            EscalationType = "Percentage",
            EscalationPercentage = 5m,
            EscalationFrequency = "Annual",
            EscalationDescription =
                "The rent may be increased by 5% annually.",
            RenewalTerms =
                "The lease may be renewed by mutual written agreement.",
            TerminationTerms =
                "Either party may terminate the lease by providing 60 days notice."
        };
    }

    private sealed class FakeLeaseAgent : ILeaseAgent
    {
        private readonly LeaseExtractionResult _result;

        public bool WasCalled { get; private set; }

        public FakeLeaseAgent(
            LeaseExtractionResult result)
        {
            _result = result;
        }

        public Task<LeaseExtractionResult> ExtractAsync(
            string documentPath,
            CancellationToken cancellationToken = default)
        {
            WasCalled = true;

            return Task.FromResult(_result);
        }
    }

    private sealed class FakeUnitMatchingService : IUnitMatchingService
    {
        private readonly UnitMatchResult _result;

        public bool WasCalled { get; private set; }

        public string? ReceivedUnitId { get; private set; }

        public FakeUnitMatchingService(
            UnitMatchResult result)
        {
            _result = result;
        }

        public Task<UnitMatchResult> MatchUnitAsync(
            string? unitId,
            CancellationToken cancellationToken = default)
        {
            WasCalled = true;
            ReceivedUnitId = unitId;

            return Task.FromResult(_result);
        }
    }

    private sealed class FakeLeaseValidationService
        : ILeaseValidationService
    {
        private readonly LeaseValidationReport _report;

        public bool WasCalled { get; private set; }

        public FakeLeaseValidationService(
            LeaseValidationReport report)
        {
            _report = report;
        }

        public Task<LeaseValidationReport> ValidateAsync(
            Lease lease,
            UnitMatchResult? unitMatchResult,
            CancellationToken cancellationToken = default)
        {
            WasCalled = true;

            return Task.FromResult(_report);
        }
    }
}