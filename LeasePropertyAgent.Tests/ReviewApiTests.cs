using System.Net;
using System.Net.Http.Json;
using LeasePropertyAgent.Application.Models;
using LeasePropertyAgent.Domain.Entities;
using LeasePropertyAgent.Domain.Enums;
using LeasePropertyAgent.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using LeasePropertyAgent.Tests.Integration;
namespace LeasePropertyAgent.Tests;

public class ReviewApiTests : IClassFixture<IntegrationTestFactory>
{
   private readonly IntegrationTestFactory _factory;


    public ReviewApiTests(IntegrationTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ReviewLeaseField_Edit_ShouldReturnNoContentAndPersistReview()
    {
        var fieldId = Guid.NewGuid();
        var leaseId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        var buildingId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            db.Properties.Add(new Property
            {
                Id = propertyId,
                Name = "Test Property"
            });

            db.Buildings.Add(new Building
            {
                Id = buildingId,
                PropertyId = propertyId,
                Name = "Test Building"
            });

            db.Units.Add(new Unit
            {
                Id = unitId,
                BuildingId = buildingId,
                UnitNumber = "TEST-1204",
                Status = UnitStatus.Available
            });

            db.Leases.Add(new Lease
            {
                Id = leaseId,
                UnitId = unitId,
                DocumentName = "test-lease.pdf",
                Status = LeaseStatus.PendingReview
            });

            db.LeaseFields.Add(new LeaseField
            {
                Id = fieldId,
                LeaseId = leaseId,
                FieldName = "MonthlyRent",
                ExtractedValue = "12000",
                Confidence = 0.95m,
                ReviewStatus = LeaseFieldReviewStatus.Pending,
                SourcePage = 2,
                SourceText = "Monthly rent: QAR 12,000"
            });
        });

        using var client = _factory.CreateClient();

        var request = new ReviewLeaseFieldRequest
        {
            Action = "edit",
            ReviewedValue = "12500",
            Reason = "Corrected after human review."
        };

        var response = await client.PostAsJsonAsync(
            $"/api/reviews/lease-fields/{fieldId}",
            request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        await VerifyAsync(async db =>
        {
            var field = await db.LeaseFields
                .FirstAsync(x => x.Id == fieldId);

            Assert.Equal("12000", field.ExtractedValue);
            Assert.Equal("12500", field.ReviewedValue);
            Assert.Equal(
                LeaseFieldReviewStatus.Edited,
                field.ReviewStatus);
            Assert.NotNull(field.ReviewedAt);

            var audit = await db.ReviewActions
                .FirstOrDefaultAsync(x =>
                    x.EntityType == "LeaseField" &&
                    x.EntityId == fieldId);

            Assert.NotNull(audit);
            Assert.Equal("edit", audit!.Action);
            Assert.Equal("12000", audit.PreviousValue);
            Assert.Equal("12500", audit.NewValue);
            Assert.Equal(
                "Corrected after human review.",
                audit.Reason);
        });
    }

    [Fact]
    public async Task ReviewLeaseFlag_Accept_ShouldReturnNoContentAndPersistReview()
    {
        var flagId = Guid.NewGuid();
        var leaseId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        var buildingId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            db.Properties.Add(new Property
            {
                Id = propertyId,
                Name = "Test Property"
            });

            db.Buildings.Add(new Building
            {
                Id = buildingId,
                PropertyId = propertyId,
                Name = "Test Building"
            });

            db.Units.Add(new Unit
            {
                Id = unitId,
                BuildingId = buildingId,
                UnitNumber = "TEST-1205",
                Status = UnitStatus.Available
            });

            db.Leases.Add(new Lease
            {
                Id = leaseId,
                UnitId = unitId,
                DocumentName = "test-lease.pdf",
                Status = LeaseStatus.PendingReview
            });

            db.LeaseFlags.Add(new LeaseFlag
            {
                Id = flagId,
                LeaseId = leaseId,
                Type = "SuspiciousValue",
                Severity = FlagSeverity.Medium,
                Message = "The extracted rent should be reviewed.",
                Status = ReviewStatus.Pending
            });
        });

        using var client = _factory.CreateClient();

        var request = new ReviewLeaseFlagRequest
        {
            Action = "accept",
            Reason = "Reviewed and confirmed."
        };

        var response = await client.PostAsJsonAsync(
            $"/api/reviews/lease-flags/{flagId}",
            request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        await VerifyAsync(async db =>
        {
            var flag = await db.LeaseFlags
                .FirstAsync(x => x.Id == flagId);

            Assert.Equal(
                ReviewStatus.Accepted,
                flag.Status);

            var audit = await db.ReviewActions
                .FirstOrDefaultAsync(x =>
                    x.EntityType == "LeaseFlag" &&
                    x.EntityId == flagId);

            Assert.NotNull(audit);
            Assert.Equal("accept", audit!.Action);
            Assert.Equal(
                "Reviewed and confirmed.",
                audit.Reason);
        });
    }

    [Fact]
    public async Task ReviewWorkOrder_Edit_ShouldReturnNoContentAndPersistReview()
    {
        var workOrderId = Guid.NewGuid();
        var issueId = Guid.NewGuid();
        var unitId = Guid.NewGuid();
        var buildingId = Guid.NewGuid();
        var propertyId = Guid.NewGuid();

        await SeedAsync(db =>
        {
            db.Properties.Add(new Property
            {
                Id = propertyId,
                Name = "Test Property"
            });

            db.Buildings.Add(new Building
            {
                Id = buildingId,
                PropertyId = propertyId,
                Name = "Test Building"
            });

            db.Units.Add(new Unit
            {
                Id = unitId,
                BuildingId = buildingId,
                UnitNumber = "TEST-0902",
                Status = UnitStatus.Occupied
            });

            db.Issues.Add(new Issue
            {
                Id = issueId,
                UnitId = unitId,
                Title = "Damaged fixture",
                Description = "Fixture requires inspection.",
                Status = ReviewStatus.Pending,
                CreatedAt = DateTime.UtcNow
            });

            db.WorkOrders.Add(new WorkOrder
            {
                Id = workOrderId,
                IssueId = issueId,
                UnitId = unitId,
                Title = "Inspect fixture",
                Description = "Inspect and determine repair.",
                Status = WorkOrderStatus.Draft,
                CreatedAt = DateTime.UtcNow
            });
        });

        using var client = _factory.CreateClient();

        var request = new ReviewWorkOrderRequest
        {
            Action = "edit",
            Title = "Repair bathroom fixture",
            Description = "Repair or replace the damaged bathroom fixture.",
            Reason = "Clarified maintenance action."
        };

        var response = await client.PostAsJsonAsync(
            $"/api/reviews/work-orders/{workOrderId}",
            request);

        Assert.Equal(HttpStatusCode.NoContent, response.StatusCode);

        await VerifyAsync(async db =>
        {
            var workOrder = await db.WorkOrders
                .FirstAsync(x => x.Id == workOrderId);

            Assert.Equal(
                "Repair bathroom fixture",
                workOrder.Title);

            Assert.Equal(
                "Repair or replace the damaged bathroom fixture.",
                workOrder.Description);

            Assert.Equal(
                WorkOrderStatus.Edited,
                workOrder.Status);

            Assert.NotNull(workOrder.ReviewedAt);

            var audit = await db.ReviewActions
                .FirstOrDefaultAsync(x =>
                    x.EntityType == "WorkOrder" &&
                    x.EntityId == workOrderId);

            Assert.NotNull(audit);
            Assert.Equal("edit", audit!.Action);
            Assert.Equal(
                "Clarified maintenance action.",
                audit.Reason);

            Assert.Contains(
                "Inspect fixture",
                audit.PreviousValue);

            Assert.Contains(
                "Repair bathroom fixture",
                audit.NewValue);
        });
    }

    [Fact]
    public async Task ReviewLeaseField_UnknownField_ShouldReturnNotFound()
    {
        using var client = _factory.CreateClient();

        var request = new ReviewLeaseFieldRequest
        {
            Action = "accept"
        };

        var response = await client.PostAsJsonAsync(
            $"/api/reviews/lease-fields/{Guid.NewGuid()}",
            request);

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    private async Task SeedAsync(Action<LeasePropertyDbContext> seed)
    {
        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<LeasePropertyDbContext>();

        await db.Database.EnsureDeletedAsync();
        await db.Database.EnsureCreatedAsync();

        seed(db);

        await db.SaveChangesAsync();
    }

    private async Task VerifyAsync(
        Func<LeasePropertyDbContext, Task> verify)
    {
        using var scope = _factory.Services.CreateScope();

        var db = scope.ServiceProvider
            .GetRequiredService<LeasePropertyDbContext>();

        await verify(db);
    }
}