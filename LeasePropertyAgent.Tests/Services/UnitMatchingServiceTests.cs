using Xunit;
using LeasePropertyAgent.Application.Services;
using LeasePropertyAgent.Application.Models;
using LeasePropertyAgent.Infrastructure.Providers;

namespace LeasePropertyAgent.Tests.Services;

public class UnitMatchingServiceTests
{
    private static string GetDataPath(string fileName)
    {
        // Locate the shared data directory without depending on the developer's
        // local machine path.
        var directory = new DirectoryInfo(AppContext.BaseDirectory);

        while (directory is not null)
        {
            var dataDirectory = Path.Combine(directory.FullName, "data");

            if (Directory.Exists(dataDirectory))
            {
                return Path.Combine(dataDirectory, fileName);
            }

            directory = directory.Parent;
        }

        throw new DirectoryNotFoundException(
            "Could not locate the solution data directory.");
    }

    [Fact]
    public async Task AvailableUnit_ShouldBeAllowedForLeaseLinking()
    {
        // Arrange
        var provider = new JsonUnitCatalogProvider(
            GetDataPath("units.json"));

        var service = new UnitMatchingService(provider);

        // Act
        var result = await service.MatchUnitAsync("MC-B-1204");

        // Assert
        Assert.True(result.Exists);
        Assert.True(result.IsAvailable);
        Assert.True(result.CanLinkLease);

        Assert.Equal("MC-B-1204", result.UnitId);
        Assert.Equal("Tower B", result.BuildingName);
        Assert.Equal("Marina Crest Residences", result.PropertyName);
    }

    [Fact]
    public async Task OccupiedUnit_ShouldNotBeAllowedForLeaseLinking()
    {
        // Arrange
        var provider = new JsonUnitCatalogProvider(
            GetDataPath("units.json"));

        var service = new UnitMatchingService(provider);

        // Act
        var result = await service.MatchUnitAsync("MC-B-1205");

        // Assert
        Assert.True(result.Exists);
        Assert.False(result.IsAvailable);
        Assert.False(result.CanLinkLease);

        Assert.Equal("occupied", result.CurrentStatus);
    }

    [Fact]
    public async Task UnknownUnit_ShouldNotBeAllowedForLeaseLinking()
    {
        // Arrange
        var provider = new JsonUnitCatalogProvider(
            GetDataPath("units.json"));

        var service = new UnitMatchingService(provider);

        // Act
        var result = await service.MatchUnitAsync("UNKNOWN-UNIT");

        // Assert
        Assert.False(result.Exists);
        Assert.False(result.IsAvailable);
        Assert.False(result.CanLinkLease);
    }

    [Fact]
    public async Task MissingUnitId_ShouldNotBeAllowedForLeaseLinking()
    {
        // Arrange
        var provider = new JsonUnitCatalogProvider(
            GetDataPath("units.json"));

        var service = new UnitMatchingService(provider);

        // Act
        var result = await service.MatchUnitAsync(string.Empty);

        // Assert
        Assert.False(result.Exists);
        Assert.False(result.IsAvailable);
        Assert.False(result.CanLinkLease);
        Assert.Contains("No unit identifier", result.Reason);
    }
}