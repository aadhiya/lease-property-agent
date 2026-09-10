using System.Net;
using System.Net.Http.Json;
using LeasePropertyAgent.Application.Models;
using LeasePropertyAgent.Domain.Entities;
using LeasePropertyAgent.Domain.Enums;
using LeasePropertyAgent.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace LeasePropertyAgent.Tests.Integration;
[Collection("IntegrationTests")]
public class IssueApiTests : IClassFixture<IntegrationTestFactory>
{
    private readonly IntegrationTestFactory _factory;

public IssueApiTests(IntegrationTestFactory factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ProcessIssue_ShouldReturnCreatedIssueWithWorkOrder()
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
                Name = "Test Property",
                Address = "Test Address"
            };

            var building = new Building
            {
                Id = Guid.NewGuid(),
                PropertyId = property.Id,
                Name = "Test Tower",
                Property = property
            };

            var unit = new Unit
            {
                Id = Guid.NewGuid(),
                BuildingId = building.Id,
                UnitNumber = "TEST-1204",
                UnitType = "2BR",
                Area = 118,
                Status = UnitStatus.Available,
                Building = building
            };

            await dbContext.Properties.AddAsync(property);
            await dbContext.Buildings.AddAsync(building);
            await dbContext.Units.AddAsync(unit);
            await dbContext.SaveChangesAsync();

            unitId = unit.Id;
        }

        var request = new IssueProcessingRequest
        {
            UnitId = unitId,
            Images =
            [
                new IssueImageInput
                {
                    FileName = "kitchen-1.jpg",
                    FilePath = "uploads/issues/kitchen-1.jpg"
                },
                new IssueImageInput
                {
                    FileName = "kitchen-2.jpg",
                    FilePath = "uploads/issues/kitchen-2.jpg"
                }
            ]
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/issues/process",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

       var result = await response.Content
    .ReadFromJsonAsync<IssueProcessingResponse>();

Assert.NotNull(result);

Assert.Equal(unitId, result.UnitId);

Assert.Equal(
    "Property condition issue",
    result.Title);

Assert.Equal(
    "Visible wear or damage should be inspected and assessed for repair.",
    result.ConditionAssessment);

Assert.Equal(0.90m, result.Confidence);

Assert.Equal(2, result.Images.Count);

Assert.NotNull(result.WorkOrder);

Assert.Equal(
    "Inspect reported property condition",
    result.WorkOrder!.Title);
    }

    [Fact]
    public async Task ProcessIssue_WithUnknownUnit_ShouldReturnNotFound()
    {
        // Arrange
        var client = _factory.CreateClient();

        var request = new IssueProcessingRequest
        {
            UnitId = Guid.NewGuid(),
            Images =
            [
                new IssueImageInput
                {
                    FileName = "bathroom.jpg",
                    FilePath = "uploads/issues/bathroom.jpg"
                }
            ]
        };

        // Act
        var response = await client.PostAsJsonAsync(
            "/api/issues/process",
            request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}