namespace LeasePropertyAgent.Application.Models;

public class UnitListResponse
{
    public Guid UnitId { get; set; }
    public string UnitNumber { get; set; } = string.Empty;
    public string? UnitType { get; set; }
    public decimal? Area { get; set; }
    public string Status { get; set; } = string.Empty;
}