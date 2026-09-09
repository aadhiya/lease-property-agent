using System.Net;
using System.Net.Http.Json;
using LeasePropertyAgent.Application.Models;
using LeasePropertyAgent.Domain.Entities;
using LeasePropertyAgent.Domain.Enums;
using LeasePropertyAgent.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace LeasePropertyAgent.Tests.Integration;

[Collection("IntegrationTests")]
public class UnitApiTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;

    public UnitApiTests(
        WebApplicationFactory<Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task GetUnits_ShouldReturnUnits()
    {
        // Arrange
        var client = _factory.CreateClient();

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

            var unit1 = new Unit
            {
                Id = Guid.NewGuid(),
                BuildingId = building.Id,
                UnitNumber = "MC-B-1204",
                UnitType = "Apartment",
                Area = 118,
                Status = UnitStatus.Available,
                Building = building
            };

            var unit2 = new Unit
            {
                Id = Guid.NewGuid(),
                BuildingId = building.Id,
                UnitNumber = "MC-B-1205",
                UnitType = "Apartment",
                Area = 121,
                Status = UnitStatus.Occupied,
                Building = building
            };

            await dbContext.Properties.AddAsync(property);
            await dbContext.Buildings.AddAsync(building);
            await dbContext.Units.AddAsync(unit1);
            await dbContext.Units.AddAsync(unit2);

            await dbContext.SaveChangesAsync();
        }

        // Act
        var response = await client.GetAsync("/api/units");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<List<UnitListResponse>>();

        Assert.NotNull(result);

        Assert.Equal(2, result.Count);

        var availableUnit = result
            .Single(unit => unit.UnitNumber == "MC-B-1204");

        Assert.NotEqual(Guid.Empty, availableUnit.UnitId);
        Assert.Equal("Apartment", availableUnit.UnitType);
        Assert.Equal(118, availableUnit.Area);
        Assert.Equal(
            UnitStatus.Available.ToString(),
            availableUnit.Status);

        var occupiedUnit = result
            .Single(unit => unit.UnitNumber == "MC-B-1205");

        Assert.NotEqual(Guid.Empty, occupiedUnit.UnitId);
        Assert.Equal("Apartment", occupiedUnit.UnitType);
        Assert.Equal(121, occupiedUnit.Area);
        Assert.Equal(
            UnitStatus.Occupied.ToString(),
            occupiedUnit.Status);
    }

    [Fact]
    public async Task GetUnits_ShouldReturnUnitsOrderedByUnitNumber()
    {
        // Arrange
        var client = _factory.CreateClient();

        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider
                .GetRequiredService<LeasePropertyDbContext>();

            await dbContext.Database.EnsureDeletedAsync();
            await dbContext.Database.EnsureCreatedAsync();

            var property = new Property
            {
                Id = Guid.NewGuid(),
                Name = "Test Property"
            };

            var building = new Building
            {
                Id = Guid.NewGuid(),
                PropertyId = property.Id,
                Name = "Test Tower"
            };

            var unit1 = new Unit
            {
                Id = Guid.NewGuid(),
                BuildingId = building.Id,
                UnitNumber = "MC-B-1205",
                Status = UnitStatus.Occupied
            };

            var unit2 = new Unit
            {
                Id = Guid.NewGuid(),
                BuildingId = building.Id,
                UnitNumber = "MC-B-1204",
                Status = UnitStatus.Available
            };

            await dbContext.Properties.AddAsync(property);
            await dbContext.Buildings.AddAsync(building);
            await dbContext.Units.AddAsync(unit1);
            await dbContext.Units.AddAsync(unit2);

            await dbContext.SaveChangesAsync();
        }

        // Act
        var response = await client.GetAsync("/api/units");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content
            .ReadFromJsonAsync<List<UnitListResponse>>();

        Assert.NotNull(result);
        Assert.Equal(2, result.Count);

        Assert.Equal("MC-B-1204", result[0].UnitNumber);
        Assert.Equal("MC-B-1205", result[1].UnitNumber);
    }
}