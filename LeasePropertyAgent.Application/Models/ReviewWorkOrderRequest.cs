namespace LeasePropertyAgent.Application.Models;

public class ReviewWorkOrderRequest
{
    public string Action { get; set; } = string.Empty;

    public string? Title { get; set; }

    public string? Description { get; set; }

    public string? Reason { get; set; }
}