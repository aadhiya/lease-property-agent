using LeasePropertyAgent.Application.Interfaces;
using LeasePropertyAgent.Application.Models;

namespace LeasePropertyAgent.Application.Services;

public class UnitMatchingService : IUnitMatchingService
{
    private readonly IUnitCatalogProvider _unitCatalogProvider;

    public UnitMatchingService(IUnitCatalogProvider unitCatalogProvider)
    {
        _unitCatalogProvider = unitCatalogProvider;
    }

    public async Task<UnitMatchResult> MatchUnitAsync(
        string? unitId,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(unitId))
        {
            return new UnitMatchResult
            {
                Exists = false,
                IsAvailable = false,
                CanLinkLease = false,
                Reason = "No unit identifier was provided."
            };
        }

        var catalog = await _unitCatalogProvider.GetCatalogAsync(
            cancellationToken);

        // Search the complete property/building/unit hierarchy because the
        // owner's source data is hierarchical rather than a flat unit list.
        foreach (var property in catalog.Properties)
        {
            foreach (var building in property.Buildings)
            {
                var unit = building.Units.FirstOrDefault(x =>
                    string.Equals(
                        x.UnitId,
                        unitId,
                        StringComparison.OrdinalIgnoreCase));

                if (unit is null)
                {
                    continue;
                }

                var isAvailable = string.Equals(
                    unit.Status,
                    "available",
                    StringComparison.OrdinalIgnoreCase);

                return new UnitMatchResult
                {
                    Exists = true,
                    IsAvailable = isAvailable,
                    CanLinkLease = isAvailable,
                    UnitId = unit.UnitId,
                    UnitLabel = unit.Label,
                    PropertyName = property.Name,
                    BuildingName = building.Name,
                    CurrentStatus = unit.Status,
                    Reason = isAvailable
                        ? "Unit exists and is available."
                        : $"Unit exists but is currently marked '{unit.Status}'."
                };
            }
        }

        // An unknown unit cannot be linked. We deliberately return a structured
        // result instead of throwing because an unknown unit is an expected
        // business outcome that R7 needs to report.
        return new UnitMatchResult
        {
            Exists = false,
            IsAvailable = false,
            CanLinkLease = false,
            UnitId = unitId,
            Reason = "Unit does not exist in the owner's unit records."
        };
    }
}