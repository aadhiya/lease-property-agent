using LeasePropertyAgent.Infrastructure.Providers;

namespace LeasePropertyAgent.Tests.Integration;

public class JsonProviderTests
{
    private static string GetDataPath(string fileName)
    {
        // Resolve the data directory from the solution/test execution location
        // instead of hard-coding a machine-specific path such as D:\LeasePropertyAgent.
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
    public async Task UnitCatalog_ShouldLoadAllUnits()
    {
        // Arrange
        var filePath = GetDataPath("units.json");

        var provider = new JsonUnitCatalogProvider(filePath);

        // Act
        var catalog = await provider.GetCatalogAsync();

        var units = catalog.Properties
            .SelectMany(property => property.Buildings)
            .SelectMany(building => building.Units)
            .ToList();

        // Assert
        Assert.Equal(
            "Marina Crest Holdings W.L.L.",
            catalog.OwnershipEntity);

        Assert.Single(catalog.Properties);

        Assert.Equal(5, units.Count);

        Assert.Contains(
            units,
            unit => unit.UnitId == "MC-B-1204");

        Assert.Contains(
            units,
            unit => unit.UnitId == "MC-A-0301");
    }

    [Fact]
    public async Task UnitCatalog_ShouldFindSpecificUnit()
    {
        // Arrange
        var filePath = GetDataPath("units.json");

        var provider = new JsonUnitCatalogProvider(filePath);

        // Act
        var unit = await provider.FindUnitAsync("MC-B-1204");

        // Assert
        Assert.NotNull(unit);

        Assert.Equal("MC-B-1204", unit.UnitId);
        Assert.Equal("Apartment 1204", unit.Label);
        Assert.Equal("2BR", unit.Type);
        Assert.Equal(118, unit.AreaSqm);
        Assert.Equal("B-77", unit.ParkingBay);
        Assert.Equal("available", unit.Status);
    }

    [Fact]
    public async Task UnitCatalog_ShouldReturnNullForUnknownUnit()
    {
        // Arrange
        var filePath = GetDataPath("units.json");

        var provider = new JsonUnitCatalogProvider(filePath);

        // Act
        var unit = await provider.FindUnitAsync("UNKNOWN-UNIT");

        // Assert
        Assert.Null(unit);
    }

    [Fact]
    public async Task OwnerRuleset_ShouldLoadAllRules()
    {
        // Arrange
        var filePath = GetDataPath("owner_ruleset.json");

        var provider = new JsonOwnerRulesetProvider(filePath);

        // Act
        var ruleset = await provider.GetRulesetAsync();

        // Assert
        Assert.Equal(
            "Marina Crest Holdings — Lease Acceptance Standards",
            ruleset.RulesetName);

        Assert.Equal("1.0", ruleset.Version);

        Assert.Equal(7, ruleset.Rules.Count);

        // The exercise defines seven owner rules, R1 through R7.
        Assert.Equal(
            new[] { "R1", "R2", "R3", "R4", "R5", "R6", "R7" },
            ruleset.Rules.Select(rule => rule.Id));
    }

    [Fact]
    public async Task OwnerRuleset_ShouldContainExpectedRuleChecks()
    {
        // Arrange
        var filePath = GetDataPath("owner_ruleset.json");

        var provider = new JsonOwnerRulesetProvider(filePath);

        // Act
        var ruleset = await provider.GetRulesetAsync();

        // Assert
        var ruleChecks = ruleset.Rules
            .ToDictionary(rule => rule.Id, rule => rule.Check);

        // These checks are intentionally kept as data from the owner ruleset.
        // The validation engine we build later will interpret these business rules
        // deterministically rather than asking the AI model to calculate them.
        Assert.Equal(
            "deposit_amount >= monthly_rent",
            ruleChecks["R1"]);

        Assert.Equal(
            "escalation_clause.is_defined == true",
            ruleChecks["R2"]);

        Assert.Equal(
            "term_months <= 36",
            ruleChecks["R3"]);

        Assert.Equal(
            "expiry_date > commencement_date AND term_months == months_between(commencement_date, expiry_date)",
            ruleChecks["R4"]);

        Assert.Equal(
            "landlord.present AND tenant.present AND landlord.signed AND tenant.signed",
            ruleChecks["R5"]);

        Assert.Equal(
            "annual_rent == monthly_rent * 12",
            ruleChecks["R6"]);

        Assert.Equal(
            "unit_id exists in units.json AND unit.status == 'available'",
            ruleChecks["R7"]);
    }
}