namespace LeasePropertyAgent.Application.Models;

/// <summary>
/// Represents the normalized output of document extraction.
///
/// The lease agent consumes this model rather than knowing whether
/// the original document came from a PDF parser, OCR engine, or
/// another document-processing service.
/// </summary>
public class ExtractedDocument
{
    public string DocumentName { get; set; } = string.Empty;

    public List<DocumentPage> Pages { get; set; } = new();
}