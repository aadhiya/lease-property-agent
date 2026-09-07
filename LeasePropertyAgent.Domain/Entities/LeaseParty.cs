using LeasePropertyAgent.Domain.Enums;

namespace LeasePropertyAgent.Domain.Entities;

public class LeaseParty
{
    public Guid Id { get; set; }

    public Guid LeaseId { get; set; }

    public PartyRole Role { get; set; }

    public string Name { get; set; } = string.Empty;

    public bool IsPresent { get; set; }

    public bool IsSigned { get; set; }

    public Lease? Lease { get; set; }
}