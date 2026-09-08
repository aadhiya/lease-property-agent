using LeasePropertyAgent.Application.Interfaces;
using LeasePropertyAgent.Application.Models;

namespace LeasePropertyAgent.Infrastructure.DocumentExtraction;

/// <summary>
/// Development implementation of document extraction.
///
/// The take-home does not require a live OCR/document AI provider,
/// so this implementation reads a page-delimited text fixture. The
/// application-facing interface remains identical to what a real PDF
/// or OCR implementation would use later.
/// </summary>
public class StubDocumentExtractor : IDocumentExtractor
{
    public async Task<ExtractedDocument> ExtractAsync(
        string documentPath,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(documentPath))
        {
            throw new ArgumentException(
                "Document path is required.",
                nameof(documentPath));
        }

        if (!File.Exists(documentPath))
        {
            throw new FileNotFoundException(
                "The lease document could not be found.",
                documentPath);
        }

        cancellationToken.ThrowIfCancellationRequested();

        var documentText = await File.ReadAllTextAsync(
            documentPath,
            cancellationToken);

        var pages = ParsePages(documentText);

        return new ExtractedDocument
        {
            DocumentName = Path.GetFileName(documentPath),
            Pages = pages
        };
    }

    /// <summary>
    /// Splits the development fixture into page-aware content.
    ///
    /// The page markers intentionally simulate the page boundaries
    /// that a real PDF/OCR provider would return.
    /// </summary>
    private static List<DocumentPage> ParsePages(string documentText)
    {
        var sections = documentText.Split(
            "Page ",
            StringSplitOptions.RemoveEmptyEntries);

        var pages = new List<DocumentPage>();

        foreach (var section in sections)
        {
            var newlineIndex = section.IndexOf(
                '\n');

            if (newlineIndex < 0)
            {
                continue;
            }

            var pageNumberText = section[..newlineIndex]
                .Trim();

            if (!int.TryParse(pageNumberText, out var pageNumber))
            {
                continue;
            }

            var pageText = section[(newlineIndex + 1)..]
                .Trim();

            pages.Add(new DocumentPage
            {
                PageNumber = pageNumber,
                Text = pageText
            });
        }

        return pages
            .OrderBy(page => page.PageNumber)
            .ToList();
    }
}