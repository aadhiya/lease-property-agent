using System.Text.Json.Serialization;

namespace LeasePropertyAgent.Application.Models;

public class BuildingCatalog
{
    [JsonPropertyName("building_id")]
    public string BuildingId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("units")]
    public List<UnitCatalogItem> Units { get; set; } = new();
}