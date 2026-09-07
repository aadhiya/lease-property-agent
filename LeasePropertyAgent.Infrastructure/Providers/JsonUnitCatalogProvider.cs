using System.Text.Json;
using LeasePropertyAgent.Application.Interfaces;
using LeasePropertyAgent.Application.Models;

namespace LeasePropertyAgent.Infrastructure.Providers;

public class JsonUnitCatalogProvider : IUnitCatalogProvider
{
    private readonly string _filePath;
    private readonly JsonSerializerOptions _jsonOptions;

    public JsonUnitCatalogProvider(string filePath)
    {
        _filePath = filePath;

        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public async Task<UnitCatalog> GetCatalogAsync(
        CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_filePath))
        {
            throw new FileNotFoundException(
                "Unit catalog file was not found.",
                _filePath);
        }

        await using var stream = File.OpenRead(_filePath);

        var catalog = await JsonSerializer.DeserializeAsync<UnitCatalog>(
            stream,
            _jsonOptions,
            cancellationToken);

        if (catalog is null)
        {
            throw new InvalidOperationException(
                "Unit catalog could not be loaded.");
        }

        return catalog;
    }

    public async Task<UnitCatalogItem?> FindUnitAsync(
        string unitId,
        CancellationToken cancellationToken = default)
    {
        var catalog = await GetCatalogAsync(cancellationToken);

        foreach (var property in catalog.Properties)
        {
            foreach (var building in property.Buildings)
            {
                var unit = building.Units.FirstOrDefault(
                    x => string.Equals(
                        x.UnitId,
                        unitId,
                        StringComparison.OrdinalIgnoreCase));

                if (unit is not null)
                {
                    return unit;
                }
            }
        }

        return null;
    }
}