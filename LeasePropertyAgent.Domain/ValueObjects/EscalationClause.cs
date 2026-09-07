namespace LeasePropertyAgent.Domain.ValueObjects;

public class EscalationClause
{
    public bool IsDefined { get; set; }

    public string? Type { get; set; }

    public decimal? Percentage { get; set; }

    public decimal? Amount { get; set; }

    public string? Frequency { get; set; }

    public string? Description { get; set; }
}