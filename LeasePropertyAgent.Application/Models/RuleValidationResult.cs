namespace LeasePropertyAgent.Application.Models;

/// <summary>
/// Represents the result of evaluating one owner rule.
/// Keeping this separate from the database entity makes the validation
/// engine independently testable before persistence is introduced.
/// </summary>
public class RuleValidationResult
{
    public string RuleId { get; set; } = string.Empty;

    public string Status { get; set; } = string.Empty;

    public string Reason { get; set; } = string.Empty;

    public string Severity { get; set; } = string.Empty;

    public int? SourcePage { get; set; }

    public string? SourceText { get; set; }
}