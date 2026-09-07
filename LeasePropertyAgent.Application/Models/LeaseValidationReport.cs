namespace LeasePropertyAgent.Application.Models;

/// <summary>
/// Contains one complete validation execution for a lease.
/// ValidationRunId allows later human corrections to be revalidated
/// without losing the history of the previous validation run.
/// </summary>
public class LeaseValidationReport
{
    public Guid ValidationRunId { get; set; } = Guid.NewGuid();

    public List<RuleValidationResult> Results { get; set; } = new();
}