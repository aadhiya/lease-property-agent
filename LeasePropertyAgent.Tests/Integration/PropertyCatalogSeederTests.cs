using LeasePropertyAgent.Application.Interfaces;
using LeasePropertyAgent.Infrastructure.Data;
using LeasePropertyAgent.Infrastructure.Providers;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace LeasePropertyAgent.Tests.Integration;

public class PropertyCatalogSeederTests
{
    [Fact]
    public async Task SeedAsync_ShouldImportPropertyBuildingsAndUnits()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<LeasePropertyDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var dbContext = new LeasePropertyDbContext(options);

        await dbContext.Database.EnsureCreatedAsync();

        // Use the existing JSON provider so this test verifies the complete
        // catalog-to-database path rather than bypassing the application model.
        var catalogProvider = new JsonUnitCatalogProvider(
            GetUnitsJsonPath());

        IPropertyCatalogSeeder seeder =
            new JsonPropertyCatalogSeeder(
                catalogProvider,
                dbContext);

        await seeder.SeedAsync();

        var properties = await dbContext.Properties
            .Include(property => property.Buildings)
            .ThenInclude(building => building.Units)
            .ToListAsync();

        var buildings = await dbContext.Buildings
            .ToListAsync();

        var units = await dbContext.Units
            .OrderBy(unit => unit.UnitNumber)
            .ToListAsync();

        Assert.Single(properties);
        Assert.Equal(2, buildings.Count);
        Assert.Equal(5, units.Count);

        var availableUnit = units.Single(
            unit => unit.UnitNumber == "MC-B-1204");

        var occupiedUnit = units.Single(
            unit => unit.UnitNumber == "MC-B-1205");

        Assert.Equal(
            Domain.Enums.UnitStatus.Available,
            availableUnit.Status);

        Assert.Equal(
            Domain.Enums.UnitStatus.Occupied,
            occupiedUnit.Status);

        Assert.Equal(
            "Marina Crest Residences",
            properties[0].Name);

        Assert.Equal(
            "Lusail Marina District, Doha",
            properties[0].Address);
    }

    [Fact]
    public async Task SeedAsync_ShouldBeIdempotent()
    {
        await using var connection = new SqliteConnection("Data Source=:memory:");
        await connection.OpenAsync();

        var options = new DbContextOptionsBuilder<LeasePropertyDbContext>()
            .UseSqlite(connection)
            .Options;

        await using var dbContext = new LeasePropertyDbContext(options);

        await dbContext.Database.EnsureCreatedAsync();

        var catalogProvider = new JsonUnitCatalogProvider(
            GetUnitsJsonPath());

        IPropertyCatalogSeeder seeder =
            new JsonPropertyCatalogSeeder(
                catalogProvider,
                dbContext);

        await seeder.SeedAsync();
        await seeder.SeedAsync();

        Assert.Equal(1, await dbContext.Properties.CountAsync());
        Assert.Equal(2, await dbContext.Buildings.CountAsync());
        Assert.Equal(5, await dbContext.Units.CountAsync());
    }

    private static string GetUnitsJsonPath()
    {
        return Path.Combine(
            AppContext.BaseDirectory,
            "..",
            "..",
            "..",
            "..",
            "data",
            "units.json");
    }
}