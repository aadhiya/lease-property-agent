using System.Text.Json.Serialization;

namespace LeasePropertyAgent.Application.Models;

public class UnitCatalogItem
{
    [JsonPropertyName("unit_id")]
    public string UnitId { get; set; } = string.Empty;

    [JsonPropertyName("label")]
    public string Label { get; set; } = string.Empty;

    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("area_sqm")]
    public decimal AreaSqm { get; set; }

    [JsonPropertyName("parking_bay")]
    public string ParkingBay { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;
}