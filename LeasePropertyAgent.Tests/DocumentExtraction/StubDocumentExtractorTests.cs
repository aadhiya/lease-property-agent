using LeasePropertyAgent.Infrastructure.DocumentExtraction;

namespace LeasePropertyAgent.Tests.DocumentExtraction;

public class StubDocumentExtractorTests
{
    [Fact]
    public async Task ExtractAsync_ShouldReadAllLeasePages()
    {
        // Arrange
        var documentPath = CreateSampleLeaseFile();

        var extractor = new StubDocumentExtractor();

        try
        {
            // Act
            var result = await extractor.ExtractAsync(documentPath);

            // Assert
            Assert.StartsWith(
    "sample-lease-",
    result.DocumentName);

Assert.EndsWith(
    ".txt",
    result.DocumentName);

            Assert.Equal(
                3,
                result.Pages.Count);
        }
        finally
        {
            File.Delete(documentPath);
        }
    }

    [Fact]
    public async Task ExtractAsync_ShouldPreservePageNumbersAndContent()
    {
        // Arrange
        var documentPath = CreateSampleLeaseFile();

        var extractor = new StubDocumentExtractor();

        try
        {
            // Act
            var result = await extractor.ExtractAsync(documentPath);

            // Assert
            Assert.Equal(
                1,
                result.Pages[0].PageNumber);

            Assert.Contains(
                "MC-B-1204",
                result.Pages[0].Text);

            Assert.Equal(
                2,
                result.Pages[1].PageNumber);

            Assert.Contains(
                "QAR 12,000",
                result.Pages[1].Text);

            Assert.Equal(
                3,
                result.Pages[2].PageNumber);

            Assert.Contains(
                "SIGNATURES",
                result.Pages[2].Text);
        }
        finally
        {
            File.Delete(documentPath);
        }
    }

    [Fact]
    public async Task ExtractAsync_ShouldThrowWhenDocumentDoesNotExist()
    {
        // Arrange
        var extractor = new StubDocumentExtractor();

        var missingPath = Path.Combine(
            Path.GetTempPath(),
            $"missing-lease-{Guid.NewGuid()}.txt");

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(
            () => extractor.ExtractAsync(missingPath));
    }

    [Fact]
    public async Task ExtractAsync_ShouldThrowWhenPathIsEmpty()
    {
        // Arrange
        var extractor = new StubDocumentExtractor();

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(
            () => extractor.ExtractAsync(string.Empty));
    }

    /// <summary>
    /// Creates an isolated fixture instead of depending on the
    /// repository's data folder, keeping the test deterministic.
    /// </summary>
    private static string CreateSampleLeaseFile()
    {
        var path = Path.Combine(
            Path.GetTempPath(),
            $"sample-lease-{Guid.NewGuid()}.txt");

        var content = """
            MARINA CREST RESIDENCES
            RESIDENTIAL LEASE AGREEMENT

            Page 1

            LANDLORD
            Marina Crest Holdings W.L.L.

            PROPERTY
            Tower B
            Apartment 1204
            Unit ID: MC-B-1204


            Page 2

            RENT
            Monthly Rent: QAR 12,000
            Annual Rent: QAR 144,000


            Page 3

            SIGNATURES

            Landlord Signature: Signed
            Tenant Signature: Signed
            """;

        File.WriteAllText(path, content);

        return path;
    }
}