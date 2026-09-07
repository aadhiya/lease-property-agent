namespace LeasePropertyAgent.Application.Models;

public class UnitMatchResult
{
    public bool Exists { get; set; }

    public bool IsAvailable { get; set; }

    public bool CanLinkLease { get; set; }

    public string UnitId { get; set; } = string.Empty;

    public string? UnitLabel { get; set; }

    public string? PropertyName { get; set; }

    public string? BuildingName { get; set; }

    public string? CurrentStatus { get; set; }

    public string Reason { get; set; } = string.Empty;
}