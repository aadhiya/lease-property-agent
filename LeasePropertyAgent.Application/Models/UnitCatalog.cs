using System.Text.Json.Serialization;

namespace LeasePropertyAgent.Application.Models;

public class UnitCatalog
{
    [JsonPropertyName("ownership_entity")]
    public string OwnershipEntity { get; set; } = string.Empty;

    [JsonPropertyName("properties")]
    public List<PropertyCatalog> Properties { get; set; } = new();
}