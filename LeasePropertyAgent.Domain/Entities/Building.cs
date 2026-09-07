namespace LeasePropertyAgent.Domain.Entities;

public class Building
{
    public Guid Id { get; set; }

    public Guid PropertyId { get; set; }

    public string Name { get; set; } = string.Empty;

    public Property? Property { get; set; }

    public List<Unit> Units { get; set; } = new();
}