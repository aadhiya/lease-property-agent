using System.Net;
using System.Net.Http.Json;
using LeasePropertyAgent.Application.Models.Workspace;
using LeasePropertyAgent.Domain.Entities;
using LeasePropertyAgent.Domain.Enums;
using LeasePropertyAgent.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LeasePropertyAgent.Tests.Integration;

[Collection("IntegrationTests")]
public class UnitWorkspaceApiTests : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory;

    public UnitWorkspaceApiTests(IntegrationTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetWorkspace_ShouldReturnCompleteUnitWorkspace()
    {
        // Arrange
        var client = _factory.CreateClient();

        Guid unitId;

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<LeasePropertyDbContext>();

            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();

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
                UnitNumber = "MC-B-1204",
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

            lease.Parties.Add(new LeaseParty
            {
                Id = Guid.NewGuid(),
                LeaseId = lease.Id,
                Role = PartyRole.Landlord,
                Name = "Marina Crest Holdings W.L.L.",
                IsPresent = true,
                IsSigned = true
            });

            lease.Parties.Add(new LeaseParty
            {
                Id = Guid.NewGuid(),
                LeaseId = lease.Id,
                Role = PartyRole.Tenant,
                Name = "Ahmed Hassan",
                IsPresent = true,
                IsSigned = true
            });

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
                FilePath = "uploads/issues/kitchen.jpg",
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

            await dbContext.Properties.AddAsync(property);
            await dbContext.Buildings.AddAsync(building);
            await dbContext.Units.AddAsync(unit);
            await dbContext.Leases.AddAsync(lease);
            await dbContext.Issues.AddAsync(issue);

            await dbContext.SaveChangesAsync();

            unitId = unit.Id;
        }

        // Act
        var response = await client.GetAsync(
            $"/api/units/{unitId}/workspace");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<UnitWorkspaceResponse>();

        Assert.NotNull(result);

        // Unit
        Assert.Equal(unitId, result.UnitId);
        Assert.Equal("MC-B-1204", result.UnitNumber);
        Assert.Equal("Apartment", result.UnitType);
        Assert.Equal(118, result.Area);
        Assert.Equal(UnitStatus.Available.ToString(), result.Status);

        // Building
        Assert.NotNull(result.Building);
        Assert.Equal("Tower B", result.Building!.Name);

        // Property
        Assert.NotNull(result.Property);
        Assert.Equal(
            "Marina Crest Residences",
            result.Property!.Name);

        Assert.Equal(
            "Lusail Marina District, Doha",
            result.Property.Address);

        // Lease
        var leaseResult = Assert.Single(result.Leases);

        Assert.Equal(
            "sample-lease.txt",
            leaseResult.DocumentName);

        Assert.Equal(
            LeaseStatus.PendingReview.ToString(),
            leaseResult.Status);

        Assert.Equal(12000, leaseResult.MonthlyRent);
        Assert.Equal(144000, leaseResult.AnnualRent);
        Assert.Equal("QAR", leaseResult.Currency);
        Assert.Equal(12000, leaseResult.DepositAmount);

        // Parties
        Assert.Equal(2, leaseResult.Parties.Count);

        var landlord = leaseResult.Parties
            .Single(p => p.Role == PartyRole.Landlord.ToString());

        Assert.Equal(
            "Marina Crest Holdings W.L.L.",
            landlord.Name);

        Assert.True(landlord.IsPresent);
        Assert.True(landlord.IsSigned);

        // Lease field evidence
        var field = Assert.Single(leaseResult.Fields);

        Assert.Equal("MonthlyRent", field.FieldName);
        Assert.Equal("12000", field.ExtractedValue);
        Assert.Equal("12000", field.ReviewedValue);
        Assert.Equal(0.95m, field.Confidence);
        Assert.Equal(2, field.SourcePage);
        Assert.Equal(
            "Monthly rent: QAR 12,000",
            field.SourceText);

        // Flag
        var flag = Assert.Single(leaseResult.Flags);

        Assert.Equal("SuspiciousValue", flag.Type);
        Assert.Equal(
            FlagSeverity.Medium.ToString(),
            flag.Severity);

        Assert.Equal(
            "Example review flag",
            flag.Message);

        Assert.Equal(2, flag.SourcePage);

        // Validation
        var validation = Assert.Single(
            leaseResult.ValidationResults);

        Assert.Equal("R1", validation.RuleId);

        Assert.Equal(
            ValidationStatus.Pass.ToString(),
            validation.Status);

        Assert.Equal(
            "Deposit meets the required minimum.",
            validation.Reason);

        Assert.Equal(2, validation.SourcePage);

        // Issue
        var issueResult = Assert.Single(result.Issues);

        Assert.Equal(
            "Kitchen damage",
            issueResult.Title);

        Assert.Equal(
            "Visible damage around the kitchen fixture.",
            issueResult.Description);

        Assert.Equal(
            "Visible wear requires inspection.",
            issueResult.ConditionAssessment);

        Assert.Equal(0.90m, issueResult.Confidence);

        // Image
        var image = Assert.Single(issueResult.Images);

        Assert.Equal(
            "kitchen.jpg",
            image.FileName);

        Assert.Equal(
            "Visible crack near fixture.",
            image.Observation);

        Assert.Equal(0.92m, image.Confidence);

        // Work order
        var workOrder = Assert.Single(
            issueResult.WorkOrders);

        Assert.Equal(
            "Inspect kitchen fixture",
            workOrder.Title);

        Assert.Equal(
            "Inspect and repair the damaged kitchen fixture.",
            workOrder.Description);

        Assert.Equal(
            WorkOrderStatus.Draft.ToString(),
            workOrder.Status);
    }

    [Fact]
public async Task GetWorkspace_WithUnknownUnit_ShouldReturnNotFound()
{
    // Arrange
    var client = _factory.CreateClient();

    using (var scope = _factory.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider
            .GetRequiredService<LeasePropertyDbContext>();

        await dbContext.Database.EnsureCreatedAsync();
    }

    var unitId = Guid.NewGuid();

    // Act
    var response = await client.GetAsync(
        $"/api/units/{unitId}/workspace");

    // Assert
    Assert.Equal(
        HttpStatusCode.NotFound,
        response.StatusCode);
}
    [Fact]
public async Task GetWorkspace_WithEmptyUnitId_ShouldReturnNotFound()
{
    // Arrange
    var client = _factory.CreateClient();

    using (var scope = _factory.Services.CreateScope())
    {
        var dbContext = scope.ServiceProvider
            .GetRequiredService<LeasePropertyDbContext>();

        await dbContext.Database.EnsureCreatedAsync();
    }

    // Act
    var response = await client.GetAsync(
        "/api/units/00000000-0000-0000-0000-000000000000/workspace");

    // Assert
    Assert.Equal(
        HttpStatusCode.NotFound,
        response.StatusCode);
}
}