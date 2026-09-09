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

        // The application layer works with the external unit number,
        // while Lease.UnitId must contain the internal database Guid.
        var databaseUnitId = Guid.NewGuid();

        var unitRepository = new FakeUnitRepository(
            new Unit
            {
                Id = databaseUnitId,
                UnitNumber = "MC-B-1204",
                UnitType = "2BR",
                Area = 118m,
                Status = UnitStatus.Available
            });

        var leaseRepository = new FakeLeaseRepository();

        var service = new LeaseProcessingService(
            leaseAgent,
            unitMatchingService,
            validationService,
            unitRepository,
            leaseRepository);

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

        // Verify that the external unit number was resolved to the
        // internal database Guid before persistence.
        Assert.True(
            unitRepository.WasCalled);

        Assert.Equal(
            "MC-B-1204",
            unitRepository.ReceivedUnitNumber);

        Assert.Equal(
            databaseUnitId,
            result.Lease.UnitId);

        // Verify that the complete lease aggregate was handed to
        // the persistence layer.
        Assert.True(
            leaseRepository.AddWasCalled);

        Assert.True(
            leaseRepository.SaveChangesWasCalled);

        Assert.Same(
            result.Lease,
            leaseRepository.AddedLease);
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

        // No database unit should be requested when the extraction
        // does not contain a unit identifier.
        var unitRepository = new FakeUnitRepository(null);
        var leaseRepository = new FakeLeaseRepository();

        var service = new LeaseProcessingService(
            leaseAgent,
            unitMatchingService,
            validationService,
            unitRepository,
            leaseRepository);

        // Act
        var result = await service.ProcessAsync(
            "sample-lease.txt");

        // Assert
        Assert.Null(
            result.Extraction.UnitId);

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

        Assert.False(
            unitRepository.WasCalled);

        // The lease is still persisted for human review even though
        // it could not be linked to a unit.
        Assert.True(
            leaseRepository.AddWasCalled);

        Assert.True(
            leaseRepository.SaveChangesWasCalled);
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

        var unitRepository = new FakeUnitRepository(
            new Unit
            {
                Id = Guid.NewGuid(),
                UnitNumber = "MC-B-1204",
                Status = UnitStatus.Available
            });

        var leaseRepository = new FakeLeaseRepository();

        var service = new LeaseProcessingService(
            leaseAgent,
            unitMatchingService,
            validationService,
            unitRepository,
            leaseRepository);

        // Act
        var result = await service.ProcessAsync(
            "sample-lease.txt");

        // Assert
        Assert.Single(
            result.Extraction.Flags);

        Assert.Equal(
            "SuspiciousValue",
            result.Extraction.Flags[0].Type);

        // Verify the flag was also mapped into the persisted domain
        // aggregate rather than existing only in the agent result.
        Assert.Single(
            result.Lease.Flags);

        Assert.Equal(
            "SuspiciousValue",
            result.Lease.Flags[0].Type);

        Assert.Equal(
            FlagSeverity.High,
            result.Lease.Flags[0].Severity);

        Assert.Equal(
            ReviewStatus.Pending,
            result.Lease.Flags[0].Status);

        Assert.True(
            leaseRepository.AddWasCalled);

        Assert.True(
            leaseRepository.SaveChangesWasCalled);
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

   private sealed class FakeUnitRepository : IUnitRepository
{
    private readonly Unit? _unit;

    public bool WasCalled { get; private set; }

    public string? ReceivedUnitNumber { get; private set; }

    public FakeUnitRepository(Unit? unit)
    {
        _unit = unit;
    }

    public Task<Unit?> GetByUnitNumberAsync(
        string unitNumber,
        CancellationToken cancellationToken = default)
    {
        WasCalled = true;
        ReceivedUnitNumber = unitNumber;

        return Task.FromResult(_unit);
    }

    public Task<Unit?> GetByIdAsync(
        Guid unitId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Unit?>(null);
    }
     public Task<Unit?> GetWorkspaceByIdAsync(
        Guid unitId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Unit?>(null);
    }
    public Task<List<Unit>> GetAllAsync(
    CancellationToken cancellationToken = default)
{
    return Task.FromResult(new List<Unit>());
}
}    private sealed class FakeLeaseRepository : ILeaseRepository
    {
        public bool AddWasCalled { get; private set; }

        public bool SaveChangesWasCalled { get; private set; }

        public Lease? AddedLease { get; private set; }

        public Task<Lease?> GetByIdAsync(
            Guid leaseId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Lease?>(null);
        }

        public Task AddAsync(
            Lease lease,
            CancellationToken cancellationToken = default)
        {
            AddWasCalled = true;
            AddedLease = lease;

            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveChangesWasCalled = true;

            return Task.CompletedTask;
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