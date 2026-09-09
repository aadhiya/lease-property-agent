namespace LeasePropertyAgent.Application.Models;

public class ReviewLeaseFieldRequest
{
    public string Action { get; set; } = string.Empty;

    public string? ReviewedValue { get; set; }

    public string? Reason { get; set; }
}