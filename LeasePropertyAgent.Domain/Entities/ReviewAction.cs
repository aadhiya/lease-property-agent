namespace LeasePropertyAgent.Domain.Entities;

public class ReviewAction
{
    public Guid Id { get; set; }

    public string EntityType { get; set; } = string.Empty;

    public Guid EntityId { get; set; }

    public string Action { get; set; } = string.Empty;

    public string? PreviousValue { get; set; }

    public string? NewValue { get; set; }

    public string? Reason { get; set; }

    public DateTime CreatedAt { get; set; }
}