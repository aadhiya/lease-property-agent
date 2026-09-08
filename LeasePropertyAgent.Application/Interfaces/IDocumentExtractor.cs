using LeasePropertyAgent.Application.Models;

namespace LeasePropertyAgent.Application.Interfaces;

/// <summary>
/// Extracts readable, page-aware content from an uploaded lease document.
///
/// The interface deliberately hides the underlying document technology
/// so that a stub can be used for the take-home exercise and a real PDF/OCR
/// implementation can be introduced later without changing the lease agent.
/// </summary>
public interface IDocumentExtractor
{
    Task<ExtractedDocument> ExtractAsync(
        string documentPath,
        CancellationToken cancellationToken = default);
}