namespace LeasePropertyAgent.Domain.Entities;

public class Property
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Address { get; set; }

    public List<Building> Buildings { get; set; } = new();
}