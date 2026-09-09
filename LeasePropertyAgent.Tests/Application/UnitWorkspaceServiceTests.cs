using LeasePropertyAgent.Application.Interfaces;
using LeasePropertyAgent.Application.Services;
using LeasePropertyAgent.Domain.Entities;
using LeasePropertyAgent.Domain.Enums;

namespace LeasePropertyAgent.Tests.Application;

public class UnitWorkspaceServiceTests
{
    [Fact]
    public async Task GetWorkspaceAsync_ReturnsWorkspaceForExistingUnit()
    {
        var unit = CreateUnitWithWorkspaceData();

        var repository = new FakeUnitRepository(unit);
        var service = new UnitWorkspaceService(repository);

        var result = await service.GetWorkspaceAsync(unit.Id);

        Assert.NotNull(result);
        Assert.Equal(unit.Id, result.UnitId);
        Assert.Equal("1204", result.UnitNumber);
        Assert.Equal("Apartment", result.UnitType);
        Assert.Equal(UnitStatus.Available.ToString(), result.Status);

        Assert.NotNull(result.Building);
        Assert.Equal("Tower B", result.Building!.Name);

        Assert.NotNull(result.Property);
        Assert.Equal("Marina Crest Residences", result.Property!.Name);

        Assert.Single(result.Leases);
        Assert.Single(result.Issues);
    }

    [Fact]
    public async Task GetWorkspaceAsync_ReturnsNullWhenUnitDoesNotExist()
    {
        var repository = new FakeUnitRepository(null);
        var service = new UnitWorkspaceService(repository);

        var result = await service.GetWorkspaceAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task GetWorkspaceAsync_ReturnsNullForEmptyUnitId()
    {
        var repository = new FakeUnitRepository(null);
        var service = new UnitWorkspaceService(repository);

        var result = await service.GetWorkspaceAsync(Guid.Empty);

        Assert.Null(result);
        Assert.False(repository.GetWorkspaceCalled);
    }

    [Fact]
    public async Task GetWorkspaceAsync_PreservesLeaseSourceEvidenceAndReviewData()
    {
        var unit = CreateUnitWithWorkspaceData();

        var repository = new FakeUnitRepository(unit);
        var service = new UnitWorkspaceService(repository);

        var result = await service.GetWorkspaceAsync(unit.Id);

        var lease = Assert.Single(result!.Leases);
        var field = Assert.Single(lease.Fields);
        var flag = Assert.Single(lease.Flags);
        var validation = Assert.Single(lease.ValidationResults);

        Assert.Equal("MonthlyRent", field.FieldName);
        Assert.Equal("12000", field.ExtractedValue);
        Assert.Equal("12000", field.ReviewedValue);
        Assert.Equal(0.95m, field.Confidence);
        Assert.Equal(2, field.SourcePage);
        Assert.Equal("Monthly rent: QAR 12,000", field.SourceText);

        Assert.Equal("SuspiciousValue", flag.Type);
        Assert.Equal(FlagSeverity.Medium.ToString(), flag.Severity);
        Assert.Equal("Example review flag", flag.Message);
        Assert.Equal(2, flag.SourcePage);
        Assert.Equal("Monthly rent: QAR 12,000", flag.SourceText);

        Assert.Equal("R1", validation.RuleId);
        Assert.Equal(ValidationStatus.Pass.ToString(), validation.Status);
        Assert.Equal("Deposit meets the required minimum.", validation.Reason);
        Assert.Equal(2, validation.SourcePage);
    }

    [Fact]
    public async Task GetWorkspaceAsync_IncludesIssueImagesAndWorkOrders()
    {
        var unit = CreateUnitWithWorkspaceData();

        var repository = new FakeUnitRepository(unit);
        var service = new UnitWorkspaceService(repository);

        var result = await service.GetWorkspaceAsync(unit.Id);

        var issue = Assert.Single(result!.Issues);
        var image = Assert.Single(issue.Images);
        var workOrder = Assert.Single(issue.WorkOrders);

        Assert.Equal("Kitchen damage", issue.Title);
        Assert.Equal("Visible damage around the kitchen fixture.", issue.Description);
        Assert.Equal("Visible wear requires inspection.", issue.ConditionAssessment);
        Assert.Equal(0.90m, issue.Confidence);

        Assert.Equal("kitchen.jpg", image.FileName);
        Assert.Equal("Visible crack near fixture.", image.Observation);
        Assert.Equal(0.92m, image.Confidence);

        Assert.Equal("Inspect kitchen fixture", workOrder.Title);
        Assert.Equal("Inspect and repair the damaged kitchen fixture.", workOrder.Description);
        Assert.Equal(WorkOrderStatus.Draft.ToString(), workOrder.Status);
    }

    private static Unit CreateUnitWithWorkspaceData()
    {
        var property = new Property
        {
            Id = Guid.NewGuid(),
            Name = "Marina Crest Residences",
            Address = "Lusail Marina District, Doha"
        };

        var building = new Building
        {
            Id = Guid.NewGuid(),
            Name = "Tower B",
            Property = property,
            PropertyId = property.Id
        };

        var unit = new Unit
        {
            Id = Guid.NewGuid(),
            BuildingId = building.Id,
            UnitNumber = "1204",
            UnitType = "Apartment",
            Area = 118,
            Status = UnitStatus.Available,
            Building = building
        };

        var lease = new Lease
        {
            Id = Guid.NewGuid(),
            UnitId = unit.Id,
            DocumentName = "sample-lease.txt",
            Status = LeaseStatus.PendingReview,
            MonthlyRent = 12000,
            AnnualRent = 144000,
            Currency = "QAR",
            DepositAmount = 12000,
            Unit = unit
        };

        lease.Fields.Add(new LeaseField
        {
            Id = Guid.NewGuid(),
            LeaseId = lease.Id,
            FieldName = "MonthlyRent",
            ExtractedValue = "12000",
            ReviewedValue = "12000",
            Confidence = 0.95m,
            SourcePage = 2,
            SourceText = "Monthly rent: QAR 12,000"
        });

        lease.Flags.Add(new LeaseFlag
        {
            Id = Guid.NewGuid(),
            LeaseId = lease.Id,
            Type = "SuspiciousValue",
            Severity = FlagSeverity.Medium,
            Message = "Example review flag",
            SourcePage = 2,
            SourceText = "Monthly rent: QAR 12,000"
        });

        lease.ValidationResults.Add(new ValidationResult
        {
            Id = Guid.NewGuid(),
            LeaseId = lease.Id,
            ValidationRunId = Guid.NewGuid(),
            RuleId = "R1",
            Status = ValidationStatus.Pass,
            Reason = "Deposit meets the required minimum.",
            SourcePage = 2,
            SourceText = "Deposit: QAR 12,000",
            CreatedAt = DateTime.UtcNow
        });

        var issue = new Issue
        {
            Id = Guid.NewGuid(),
            UnitId = unit.Id,
            Title = "Kitchen damage",
            Description = "Visible damage around the kitchen fixture.",
            ConditionAssessment = "Visible wear requires inspection.",
            Confidence = 0.90m,
            Status = ReviewStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            Unit = unit
        };

        issue.Images.Add(new IssueImage
        {
            Id = Guid.NewGuid(),
            IssueId = issue.Id,
            FileName = "kitchen.jpg",
            FilePath = "uploads/kitchen.jpg",
            Observation = "Visible crack near fixture.",
            Confidence = 0.92m
        });

        issue.WorkOrders.Add(new WorkOrder
        {
            Id = Guid.NewGuid(),
            IssueId = issue.Id,
            UnitId = unit.Id,
            Title = "Inspect kitchen fixture",
            Description = "Inspect and repair the damaged kitchen fixture.",
            Status = WorkOrderStatus.Draft,
            CreatedAt = DateTime.UtcNow
        });

        lease.Parties.Add(new LeaseParty
        {
            Id = Guid.NewGuid(),
            LeaseId = lease.Id,
            Role = PartyRole.Landlord,
            Name = "Marina Crest Holdings W.L.L.",
            IsPresent = true,
            IsSigned = true
        });

        unit.Leases.Add(lease);
        unit.Issues.Add(issue);

        building.Units.Add(unit);
        property.Buildings.Add(building);

        return unit;
    }

    private sealed class FakeUnitRepository : IUnitRepository
    {
        private readonly Unit? _unit;

        public bool GetWorkspaceCalled { get; private set; }

        public FakeUnitRepository(Unit? unit)
        {
            _unit = unit;
        }

        public Task<Unit?> GetByUnitNumberAsync(
            string unitNumber,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult<Unit?>(null);
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
            GetWorkspaceCalled = true;

            if (_unit is not null && _unit.Id == unitId)
            {
                return Task.FromResult<Unit?>(_unit);
            }

            return Task.FromResult<Unit?>(null);
        }

        public Task<List<Unit>> GetAllAsync(
    CancellationToken cancellationToken = default)
{
    return Task.FromResult(new List<Unit>());
}
    }
}