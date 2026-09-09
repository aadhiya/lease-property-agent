using LeasePropertyAgent.Application.Interfaces;
using LeasePropertyAgent.Application.Models;
using LeasePropertyAgent.Application.Services;
using LeasePropertyAgent.Domain.Entities;
using LeasePropertyAgent.Domain.Enums;
using LeasePropertyAgent.Infrastructure.IssueAgents;

namespace LeasePropertyAgent.Tests.Services;

public class IssueProcessingServiceTests
{
    [Fact]
    public async Task ProcessAsync_ShouldMapAgentAssessmentToIssue()
    {
        // Arrange
        var unitId = Guid.NewGuid();

        var unit = new Unit
        {
            Id = unitId,
            UnitNumber = "MC-B-1204",
            Status = UnitStatus.Available
        };

        var agent = new FakeIssueAgent();
        var repository = new FakeIssueRepository();

        var service = new IssueProcessingService(
            agent,
            repository,
            new FakeUnitRepository(unit));

        var request = CreateRequest(unitId);

        // Act
        var result = await service.ProcessAsync(request);

        // Assert
        Assert.NotNull(result.Issue);
        Assert.Equal(unitId, result.Issue.UnitId);
        Assert.Equal("Water damage under kitchen sink", result.Issue.Title);
        Assert.Equal(
            "Visible water staining and cabinet damage.",
            result.Issue.Description);
        Assert.Equal(
            "The area shows visible water-related damage requiring inspection.",
            result.Issue.ConditionAssessment);
        Assert.Equal(0.92m, result.Issue.Confidence);
    }

    [Fact]
    public async Task ProcessAsync_ShouldMapImageEvidence()
    {
        // Arrange
        var unitId = Guid.NewGuid();

        var unit = new Unit
        {
            Id = unitId,
            UnitNumber = "MC-B-1204",
            Status = UnitStatus.Available
        };

        var agent = new FakeIssueAgent();
        var repository = new FakeIssueRepository();

        var service = new IssueProcessingService(
            agent,
            repository,
            new FakeUnitRepository(unit));

        var request = CreateRequest(unitId);

        // Act
        var result = await service.ProcessAsync(request);

        // Assert
        Assert.Equal(2, result.Issue.Images.Count);

        var kitchenImage = result.Issue.Images
            .Single(x => x.FileName == "kitchen.jpg");

        Assert.Equal(
            "Water staining is visible below the kitchen sink.",
            kitchenImage.Observation);

        Assert.Equal(0.94m, kitchenImage.Confidence);

        Assert.Equal(
            "kitchen.jpg",
            kitchenImage.FileName);
    }

    [Fact]
    public async Task ProcessAsync_ShouldCreateDraftWorkOrder()
    {
        // Arrange
        var unitId = Guid.NewGuid();

        var unit = new Unit
        {
            Id = unitId,
            UnitNumber = "MC-B-1204",
            Status = UnitStatus.Available
        };

        var agent = new FakeIssueAgent();
        var repository = new FakeIssueRepository();

        var service = new IssueProcessingService(
            agent,
            repository,
            new FakeUnitRepository(unit));

        var request = CreateRequest(unitId);

        // Act
        var result = await service.ProcessAsync(request);

        // Assert
        var workOrder = Assert.Single(result.Issue.WorkOrders);

        Assert.Equal(
            "Inspect and repair kitchen sink area",
            workOrder.Title);

        Assert.Equal(
            "Inspect the sink area for water leakage and repair the damaged cabinet.",
            workOrder.Description);

        Assert.Equal(unitId, workOrder.UnitId);
        Assert.Equal(result.Issue.Id, workOrder.IssueId);
        Assert.Equal(WorkOrderStatus.Draft, workOrder.Status);
    }

    [Fact]
    public async Task ProcessAsync_ShouldStartIssueInPendingReview()
    {
        // Arrange
        var unitId = Guid.NewGuid();

        var unit = new Unit
        {
            Id = unitId,
            UnitNumber = "MC-B-1204",
            Status = UnitStatus.Available
        };

        var service = new IssueProcessingService(
            new FakeIssueAgent(),
            new FakeIssueRepository(),
            new FakeUnitRepository(unit));

        // Act
        var result = await service.ProcessAsync(CreateRequest(unitId));

        // Assert
        Assert.Equal(ReviewStatus.Pending, result.Issue.Status);
    }

    [Fact]
    public async Task ProcessAsync_ShouldPersistIssue()
    {
        // Arrange
        var unitId = Guid.NewGuid();

        var unit = new Unit
        {
            Id = unitId,
            UnitNumber = "MC-B-1204",
            Status = UnitStatus.Available
        };

        var repository = new FakeIssueRepository();

        var service = new IssueProcessingService(
            new FakeIssueAgent(),
            repository,
            new FakeUnitRepository(unit));

        // Act
        await service.ProcessAsync(CreateRequest(unitId));

        // Assert
        Assert.True(repository.AddWasCalled);
        Assert.True(repository.SaveWasCalled);
        Assert.NotNull(repository.SavedIssue);
    }

    [Fact]
    public async Task ProcessAsync_ShouldRejectUnknownUnit()
    {
        // Arrange
        var unitId = Guid.NewGuid();

        var agent = new FakeIssueAgent();
        var repository = new FakeIssueRepository();

        var service = new IssueProcessingService(
            agent,
            repository,
            new FakeUnitRepository(null));

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(
            () => service.ProcessAsync(CreateRequest(unitId)));

        Assert.False(agent.WasCalled);
        Assert.False(repository.AddWasCalled);
    }

    private static IssueProcessingRequest CreateRequest(Guid unitId)
    {
        return new IssueProcessingRequest
        {
            UnitId = unitId,
            Images =
            {
                new IssueImageInput
                {
                    FileName = "kitchen.jpg",
                    FilePath = "test-data/kitchen.jpg"
                },
                new IssueImageInput
                {
                    FileName = "sink.jpg",
                    FilePath = "test-data/sink.jpg"
                }
            }
        };
    }

    private sealed class FakeIssueAgent : IIssueAgent
    {
        public bool WasCalled { get; private set; }

        public Task<IssueAgentResult> AssessAsync(
            IReadOnlyList<IssueImageInput> images,
            CancellationToken cancellationToken = default)
        {
            WasCalled = true;

            return Task.FromResult(
                new IssueAgentResult
                {
                    Assessment = new IssueAssessment
                    {
                        Title = "Water damage under kitchen sink",
                        Description =
                            "Visible water staining and cabinet damage.",
                        ConditionAssessment =
                            "The area shows visible water-related damage requiring inspection.",
                        Confidence = 0.92m,
                        ImageAssessments =
                        {
                            new IssueImageAssessment
                            {
                                FileName = "kitchen.jpg",
                                Observation =
                                    "Water staining is visible below the kitchen sink.",
                                Confidence = 0.94m,
                                VisibleItems =
                                {
                                    "Kitchen sink",
                                    "Under-sink cabinet"
                                }
                            },
                            new IssueImageAssessment
                            {
                                FileName = "sink.jpg",
                                Observation =
                                    "Possible leakage is visible around the sink plumbing.",
                                Confidence = 0.89m,
                                VisibleItems =
                                {
                                    "Sink",
                                    "Plumbing connection"
                                }
                            }
                        }
                    },
                    WorkOrder = new WorkOrderDraft
                    {
                        Title = "Inspect and repair kitchen sink area",
                        Description =
                            "Inspect the sink area for water leakage and repair the damaged cabinet."
                    }
                });
        }
    }

    private sealed class FakeIssueRepository : IIssueRepository
    {
        public bool AddWasCalled { get; private set; }

        public bool SaveWasCalled { get; private set; }

        public Issue? SavedIssue { get; private set; }

        public Task AddAsync(
            Issue issue,
            CancellationToken cancellationToken = default)
        {
            AddWasCalled = true;
            SavedIssue = issue;

            return Task.CompletedTask;
        }

        public Task SaveChangesAsync(
            CancellationToken cancellationToken = default)
        {
            SaveWasCalled = true;

            return Task.CompletedTask;
        }
    }

    private sealed class FakeUnitRepository : IUnitRepository
    {
        private readonly Unit? _unit;

        public FakeUnitRepository(Unit? unit)
        {
            _unit = unit;
        }

        public Task<Unit?> GetByUnitNumberAsync(
            string unitNumber,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_unit);
        }

        public Task<Unit?> GetByIdAsync(
            Guid unitId,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(_unit);
        }
         public Task<Unit?> GetWorkspaceByIdAsync(
        Guid unitId,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult<Unit?>(null);
    }
    }
}