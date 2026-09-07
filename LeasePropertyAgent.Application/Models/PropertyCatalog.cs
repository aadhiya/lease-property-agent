using System.Text.Json.Serialization;

namespace LeasePropertyAgent.Application.Models;

public class PropertyCatalog
{
    [JsonPropertyName("property_id")]
    public string PropertyId { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("location")]
    public string Location { get; set; } = string.Empty;

    [JsonPropertyName("buildings")]
    public List<BuildingCatalog> Buildings { get; set; } = new();
}