using LeasePropertyAgent.Application.Interfaces;
using LeasePropertyAgent.Application.Models;
using LeasePropertyAgent.Application.Services;
using LeasePropertyAgent.Domain.Entities;
using LeasePropertyAgent.Domain.Enums;
using LeasePropertyAgent.Infrastructure.Data;
using LeasePropertyAgent.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LeasePropertyAgent.Tests.Integration;

public class ReviewPersistenceTests
{
    [Fact]
    public async Task ReviewLeaseField_Edit_ShouldPersistReviewedValueAndAuditAction()
    {
        // Arrange
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<LeasePropertyDbContext>()
            .UseSqlite(connection)
            .Options;

        Guid fieldId;

        await using (var context = new LeasePropertyDbContext(options))
        {
            await context.Database.EnsureCreatedAsync();

            var property = new Property
            {
                Id = Guid.NewGuid(),
                Name = "Test Property"
            };

            var building = new Building
            {
                Id = Guid.NewGuid(),
                Name = "Test Tower",
                PropertyId = property.Id
            };

            var unit = new Unit
            {
                Id = Guid.NewGuid(),
                UnitNumber = "TEST-1204",
                BuildingId = building.Id,
                Status = UnitStatus.Occupied
            };

            var lease = new Lease
            {
                Id = Guid.NewGuid(),
                UnitId = unit.Id,
                DocumentName = "test-lease.txt",
                Status = LeaseStatus.PendingReview,
                CreatedAt = DateTime.UtcNow
            };

            var field = new LeaseField
            {
                Id = Guid.NewGuid(),
                LeaseId = lease.Id,
                FieldName = "MonthlyRent",
                ExtractedValue = "12000",
                Confidence = 0.95m,
                ReviewStatus = LeaseFieldReviewStatus.Pending,
                SourcePage = 2,
                SourceText = "Monthly rent: QAR 12,000"
            };

            await context.Properties.AddAsync(property);
            await context.Buildings.AddAsync(building);
            await context.Units.AddAsync(unit);
            await context.Leases.AddAsync(lease);
            await context.LeaseFields.AddAsync(field);
            await context.SaveChangesAsync();

            fieldId = field.Id;
        }

        await using (var context = new LeasePropertyDbContext(options))
        {
            var repository = new ReviewRepository(context);
            var service = new ReviewService(repository);

            var request = new ReviewLeaseFieldRequest
            {
                Action = "edit",
                ReviewedValue = "12500",
                Reason = "Corrected after human review."
            };

            // Act
            await service.ReviewLeaseFieldAsync(fieldId, request);
        }

        // Assert
        await using (var verificationContext =
            new LeasePropertyDbContext(options))
        {
            var field = await verificationContext.LeaseFields
                .FirstAsync(x => x.Id == fieldId);

            Assert.Equal(
                "12000",
                field.ExtractedValue);

            Assert.Equal(
                "12500",
                field.ReviewedValue);

            Assert.Equal(
                LeaseFieldReviewStatus.Edited,
                field.ReviewStatus);

            Assert.NotNull(field.ReviewedAt);

            var reviewAction = await verificationContext.ReviewActions
                .FirstOrDefaultAsync(x =>
                    x.EntityType == "LeaseField" &&
                    x.EntityId == fieldId);

            Assert.NotNull(reviewAction);

            Assert.Equal("edit", reviewAction.Action);
            Assert.Equal("12000", reviewAction.PreviousValue);
            Assert.Equal("12500", reviewAction.NewValue);
            Assert.Equal(
                "Corrected after human review.",
                reviewAction.Reason);
        }
    }

    [Fact]
    public async Task ReviewLeaseFlag_Accept_ShouldPersistStatusAndAuditAction()
    {
        // Arrange
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<LeasePropertyDbContext>()
            .UseSqlite(connection)
            .Options;

        Guid flagId;

        await using (var context = new LeasePropertyDbContext(options))
        {
            await context.Database.EnsureCreatedAsync();

            var property = new Property
            {
                Id = Guid.NewGuid(),
                Name = "Test Property"
            };

            var building = new Building
            {
                Id = Guid.NewGuid(),
                Name = "Test Tower",
                PropertyId = property.Id
            };

            var unit = new Unit
            {
                Id = Guid.NewGuid(),
                UnitNumber = "TEST-1205",
                BuildingId = building.Id,
                Status = UnitStatus.Occupied
            };

            var lease = new Lease
            {
                Id = Guid.NewGuid(),
                UnitId = unit.Id,
                DocumentName = "test-lease.txt",
                Status = LeaseStatus.PendingReview,
                CreatedAt = DateTime.UtcNow
            };

            var flag = new LeaseFlag
            {
                Id = Guid.NewGuid(),
                LeaseId = lease.Id,
                Type = "SuspiciousValue",
                Severity = FlagSeverity.Medium,
                Message = "The extracted value should be reviewed.",
                Status = ReviewStatus.Pending
            };

            await context.Properties.AddAsync(property);
            await context.Buildings.AddAsync(building);
            await context.Units.AddAsync(unit);
            await context.Leases.AddAsync(lease);
            await context.LeaseFlags.AddAsync(flag);
            await context.SaveChangesAsync();

            flagId = flag.Id;
        }

        await using (var context = new LeasePropertyDbContext(options))
        {
            var repository = new ReviewRepository(context);
            var service = new ReviewService(repository);

            var request = new ReviewLeaseFlagRequest
            {
                Action = "accept",
                Reason = "Reviewed and confirmed."
            };

            // Act
            await service.ReviewLeaseFlagAsync(flagId, request);
        }

        // Assert
        await using (var verificationContext =
            new LeasePropertyDbContext(options))
        {
            var flag = await verificationContext.LeaseFlags
                .FirstAsync(x => x.Id == flagId);

            Assert.Equal(
                ReviewStatus.Accepted,
                flag.Status);

            var reviewAction = await verificationContext.ReviewActions
                .FirstOrDefaultAsync(x =>
                    x.EntityType == "LeaseFlag" &&
                    x.EntityId == flagId);

            Assert.NotNull(reviewAction);

            Assert.Equal("accept", reviewAction.Action);
            Assert.Equal(
                "Reviewed and confirmed.",
                reviewAction.Reason);
        }
    }

    [Fact]
    public async Task ReviewWorkOrder_Edit_ShouldPersistChangesAndAuditAction()
    {
        // Arrange
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<LeasePropertyDbContext>()
            .UseSqlite(connection)
            .Options;

        Guid workOrderId;

        await using (var context = new LeasePropertyDbContext(options))
        {
            await context.Database.EnsureCreatedAsync();

            var property = new Property
            {
                Id = Guid.NewGuid(),
                Name = "Test Property"
            };

            var building = new Building
            {
                Id = Guid.NewGuid(),
                Name = "Test Tower",
                PropertyId = property.Id
            };

            var unit = new Unit
            {
                Id = Guid.NewGuid(),
                UnitNumber = "TEST-1204",
                BuildingId = building.Id,
                Status = UnitStatus.Occupied
            };

            var issue = new Issue
            {
                Id = Guid.NewGuid(),
                UnitId = unit.Id,
                Title = "Damaged cabinet",
                Description = "Cabinet requires inspection.",
                Status = ReviewStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };

            var workOrder = new WorkOrder
            {
                Id = Guid.NewGuid(),
                IssueId = issue.Id,
                UnitId = unit.Id,
                Title = "Inspect cabinet",
                Description = "Inspect the damaged cabinet.",
                Status = WorkOrderStatus.Draft,
                CreatedAt = DateTime.UtcNow
            };

            await context.Properties.AddAsync(property);
            await context.Buildings.AddAsync(building);
            await context.Units.AddAsync(unit);
            await context.Issues.AddAsync(issue);
            await context.WorkOrders.AddAsync(workOrder);
            await context.SaveChangesAsync();

            workOrderId = workOrder.Id;
        }

        await using (var context = new LeasePropertyDbContext(options))
        {
            var repository = new ReviewRepository(context);
            var service = new ReviewService(repository);

            var request = new ReviewWorkOrderRequest
            {
                Action = "edit",
                Title = "Repair kitchen cabinet",
                Description = "Repair or replace the damaged cabinet door.",
                Reason = "Updated scope after human review."
            };

            // Act
            await service.ReviewWorkOrderAsync(
                workOrderId,
                request);
        }

        // Assert
        await using (var verificationContext =
            new LeasePropertyDbContext(options))
        {
            var workOrder = await verificationContext.WorkOrders
                .FirstAsync(x => x.Id == workOrderId);

            Assert.Equal(
                "Repair kitchen cabinet",
                workOrder.Title);

            Assert.Equal(
                "Repair or replace the damaged cabinet door.",
                workOrder.Description);

            Assert.Equal(
                WorkOrderStatus.Edited,
                workOrder.Status);

            Assert.NotNull(workOrder.ReviewedAt);

            var reviewAction = await verificationContext.ReviewActions
                .FirstOrDefaultAsync(x =>
                    x.EntityType == "WorkOrder" &&
                    x.EntityId == workOrderId);

            Assert.NotNull(reviewAction);

            Assert.Equal("edit", reviewAction.Action);
            Assert.Contains("Inspect cabinet", reviewAction.PreviousValue);
            Assert.Contains(
                "Repair kitchen cabinet",
                reviewAction.NewValue);
            Assert.Equal(
                "Updated scope after human review.",
                reviewAction.Reason);
        }
    }
}