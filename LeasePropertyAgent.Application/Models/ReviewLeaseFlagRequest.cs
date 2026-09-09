namespace LeasePropertyAgent.Application.Models;

public class ReviewLeaseFlagRequest
{
    public string Action { get; set; } = string.Empty;

    public string? Reason { get; set; }
}