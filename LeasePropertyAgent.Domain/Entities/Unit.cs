using LeasePropertyAgent.Domain.Enums;

namespace LeasePropertyAgent.Domain.Entities;

public class Unit
{
    public Guid Id { get; set; }

    public Guid BuildingId { get; set; }

    public string UnitNumber { get; set; } = string.Empty;

    public string? UnitType { get; set; }

    public decimal? Area { get; set; }

    public UnitStatus Status { get; set; }

    public Building? Building { get; set; }

    public List<Lease> Leases { get; set; } = new();

    public List<Issue> Issues { get; set; } = new();
}