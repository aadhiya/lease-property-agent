using LeasePropertyAgent.Domain.Entities;
using LeasePropertyAgent.Domain.Enums;
using LeasePropertyAgent.Infrastructure.Data;
using LeasePropertyAgent.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace LeasePropertyAgent.Tests.Integration;

public class IssuePersistenceTests
{
    [Fact]
    public async Task AddAsync_ShouldPersistIssueImagesAndWorkOrder()
    {
        // Arrange
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<LeasePropertyDbContext>()
            .UseSqlite(connection)
            .Options;

        await using (var setupContext = new LeasePropertyDbContext(options))
        {
            await setupContext.Database.EnsureCreatedAsync();

            var property = new Property
            {
                Id = Guid.NewGuid(),
                Name = "Marina Crest Residences",
                Address = "Lusail Marina District, Doha"
            };

            var building = new Building
            {
                Id = Guid.NewGuid(),
                PropertyId = property.Id,
                Name = "Tower B",
                Property = property
            };

            var unit = new Unit
            {
                Id = Guid.NewGuid(),
                BuildingId = building.Id,
                UnitNumber = "1204",
                UnitType = "2BR",
                Area = 118,
                Status = UnitStatus.Occupied,
                Building = building
            };

            await setupContext.Properties.AddAsync(property);
            await setupContext.Buildings.AddAsync(building);
            await setupContext.Units.AddAsync(unit);
            await setupContext.SaveChangesAsync();

            var issue = new Issue
            {
                Id = Guid.NewGuid(),
                UnitId = unit.Id,
                Unit = unit,
                Title = "Damaged kitchen cabinet",
                Description = "The kitchen cabinet door appears damaged.",
                ConditionAssessment =
                    "Visible damage is present on the cabinet door and should be inspected.",
                Confidence = 0.90m,
                CreatedAt = DateTime.UtcNow,
                Status = ReviewStatus.Pending
            };

            issue.Images.Add(new IssueImage
            {
                Id = Guid.NewGuid(),
                IssueId = issue.Id,
                FileName = "kitchen-1.jpg",
                FilePath = "uploads/issues/kitchen-1.jpg",
                Observation = "Visible damage on the cabinet door.",
                Confidence = 0.92m,
                Issue = issue
            });

            issue.Images.Add(new IssueImage
            {
                Id = Guid.NewGuid(),
                IssueId = issue.Id,
                FileName = "kitchen-2.jpg",
                FilePath = "uploads/issues/kitchen-2.jpg",
                Observation = "Adjacent surface shows visible wear.",
                Confidence = 0.87m,
                Issue = issue
            });

            issue.WorkOrders.Add(new WorkOrder
            {
                Id = Guid.NewGuid(),
                IssueId = issue.Id,
                UnitId = unit.Id,
                Title = "Inspect damaged kitchen cabinet",
                Description =
                    "Inspect the cabinet door and determine the required repair.",
                Status = WorkOrderStatus.Draft,
                CreatedAt = DateTime.UtcNow,
                Issue = issue,
                Unit = unit
            });

            var repository = new IssueRepository(setupContext);

            // Act
            await repository.AddAsync(issue);
            await repository.SaveChangesAsync();
        }

        // Assert using a fresh DbContext to prove the data was actually persisted.
        await using var verificationContext =
            new LeasePropertyDbContext(options);

        var persistedIssue = await verificationContext.Issues
            .Include(x => x.Unit)
            .Include(x => x.Images)
            .Include(x => x.WorkOrders)
            .FirstOrDefaultAsync();

        Assert.NotNull(persistedIssue);

        Assert.Equal("Damaged kitchen cabinet", persistedIssue.Title);
        Assert.Equal(
            "The kitchen cabinet door appears damaged.",
            persistedIssue.Description);

        Assert.Equal(
            "Visible damage is present on the cabinet door and should be inspected.",
            persistedIssue.ConditionAssessment);

        Assert.Equal(0.90m, persistedIssue.Confidence);
        Assert.Equal(ReviewStatus.Pending, persistedIssue.Status);

        Assert.NotNull(persistedIssue.Unit);
        Assert.Equal("1204", persistedIssue.Unit!.UnitNumber);

        Assert.Equal(2, persistedIssue.Images.Count);

        var firstImage = persistedIssue.Images
            .Single(x => x.FileName == "kitchen-1.jpg");

        Assert.Equal(
            "uploads/issues/kitchen-1.jpg",
            firstImage.FilePath);

        Assert.Equal(
            "Visible damage on the cabinet door.",
            firstImage.Observation);

        Assert.Equal(0.92m, firstImage.Confidence);

        var secondImage = persistedIssue.Images
            .Single(x => x.FileName == "kitchen-2.jpg");

        Assert.Equal(
            "Adjacent surface shows visible wear.",
            secondImage.Observation);

        Assert.Equal(0.87m, secondImage.Confidence);

        Assert.Single(persistedIssue.WorkOrders);

        var workOrder = persistedIssue.WorkOrders.Single();

        Assert.Equal(
            "Inspect damaged kitchen cabinet",
            workOrder.Title);

        Assert.Equal(
            "Inspect the cabinet door and determine the required repair.",
            workOrder.Description);

        Assert.Equal(
            WorkOrderStatus.Draft,
            workOrder.Status);

        Assert.Equal(
            persistedIssue.UnitId,
            workOrder.UnitId);
    }
}