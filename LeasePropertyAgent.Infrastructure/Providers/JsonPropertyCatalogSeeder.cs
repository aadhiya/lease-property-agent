using LeasePropertyAgent.Application.Interfaces;
using LeasePropertyAgent.Domain.Entities;
using LeasePropertyAgent.Domain.Enums;
using LeasePropertyAgent.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace LeasePropertyAgent.Infrastructure.Providers;

/// <summary>
/// Synchronizes the configured JSON property catalog into the relational
/// database so extracted leases can be linked to real property units.
/// </summary>
public class JsonPropertyCatalogSeeder : IPropertyCatalogSeeder
{
    private readonly IUnitCatalogProvider _catalogProvider;
    private readonly LeasePropertyDbContext _dbContext;

    public JsonPropertyCatalogSeeder(
        IUnitCatalogProvider catalogProvider,
        LeasePropertyDbContext dbContext)
    {
        _catalogProvider = catalogProvider;
        _dbContext = dbContext;
    }

    public async Task SeedAsync(
        CancellationToken cancellationToken = default)
    {
        var catalog = await _catalogProvider.GetCatalogAsync(
            cancellationToken);

        foreach (var propertyCatalog in catalog.Properties)
        {
            var property = await _dbContext.Properties
                .Include(p => p.Buildings)
                .ThenInclude(b => b.Units)
                .FirstOrDefaultAsync(
                    p => p.Name == propertyCatalog.Name,
                    cancellationToken);

            if (property == null)
            {
                property = new Property
                {
                    Id = Guid.NewGuid(),
                    Name = propertyCatalog.Name,
                    Address = propertyCatalog.Location
                };

                await _dbContext.Properties.AddAsync(
                    property,
                    cancellationToken);
            }
            else
            {
                // Preserve the existing database identity while keeping
                // catalog-managed property information synchronized.
                property.Address = propertyCatalog.Location;
            }

            foreach (var buildingCatalog in propertyCatalog.Buildings)
            {
                var building = property.Buildings
                    .FirstOrDefault(
                        b => b.Name == buildingCatalog.Name);

                if (building == null)
                {
                    building = new Building
                    {
                        Id = Guid.NewGuid(),
                        PropertyId = property.Id,
                        Name = buildingCatalog.Name
                    };

                    property.Buildings.Add(building);
                }

                foreach (var unitCatalog in buildingCatalog.Units)
                {
                    var unit = building.Units
                        .FirstOrDefault(
                            u => u.UnitNumber == unitCatalog.UnitId);

                    if (unit == null)
                    {
                        unit = new Unit
                        {
                            Id = Guid.NewGuid(),
                            BuildingId = building.Id,
                            UnitNumber = unitCatalog.UnitId
                        };

                        building.Units.Add(unit);
                    }

                    unit.UnitType = unitCatalog.Type;
                    unit.Area = unitCatalog.AreaSqm;
                    unit.Status = MapUnitStatus(unitCatalog.Status);
                }
            }
        }

        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private static UnitStatus MapUnitStatus(string? status)
    {
        if (string.Equals(
                status,
                "available",
                StringComparison.OrdinalIgnoreCase))
        {
            return UnitStatus.Available;
        }

        if (string.Equals(
                status,
                "occupied",
                StringComparison.OrdinalIgnoreCase))
        {
            return UnitStatus.Occupied;
        }

        if (string.Equals(
                status,
                "maintenance",
                StringComparison.OrdinalIgnoreCase))
        {
            return UnitStatus.Maintenance;
        }

        // Never treat an unknown catalog value as an available unit.
        return UnitStatus.Inactive;
    }
}