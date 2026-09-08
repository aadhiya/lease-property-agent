namespace LeasePropertyAgent.Application.Models;

/// <summary>
/// Represents the text extracted from one document page.
///
/// Keeping the page boundary is important because every lease field
/// needs to remain traceable back to its original source location.
/// </summary>
public class DocumentPage
{
    public int PageNumber { get; set; }

    public string Text { get; set; } = string.Empty;
}