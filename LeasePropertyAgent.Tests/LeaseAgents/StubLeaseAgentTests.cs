using LeasePropertyAgent.Infrastructure.DocumentExtraction;
using LeasePropertyAgent.Infrastructure.LeaseAgents;

namespace LeasePropertyAgent.Tests.LeaseAgents;

public class StubLeaseAgentTests
{
    [Fact]
    public async Task ExtractAsync_ShouldExtractStructuredLeaseData()
    {
        // Arrange
        var documentPath = CreateSampleLeaseFile();

        var documentExtractor = new StubDocumentExtractor();
        var agent = new StubLeaseAgent(documentExtractor);

        try
        {
            // Act
            var result = await agent.ExtractAsync(documentPath);

            // Assert
            Assert.Equal(
                "MC-B-1204",
                result.UnitId);

            Assert.Equal(
                "Marina Crest Holdings W.L.L.",
                result.LandlordName);

            Assert.Equal(
                "Ahmed Hassan",
                result.TenantName);

            Assert.True(result.LandlordPresent);
            Assert.True(result.TenantPresent);
            Assert.True(result.LandlordSigned);
            Assert.True(result.TenantSigned);

            Assert.Equal(
                new DateTime(2026, 1, 1),
                result.CommencementDate);

            Assert.Equal(
                new DateTime(2027, 12, 31),
                result.ExpiryDate);

            Assert.Equal(
                24,
                result.TermMonths);

            Assert.Equal(
                12000m,
                result.MonthlyRent);

            Assert.Equal(
                144000m,
                result.AnnualRent);

            Assert.Equal(
                "Monthly",
                result.RentFrequency);

            Assert.Equal(
                "QAR",
                result.Currency);

            Assert.Equal(
                12000m,
                result.DepositAmount);

            Assert.True(result.EscalationIsDefined);

            Assert.Equal(
                "Percentage",
                result.EscalationType);

            Assert.Equal(
                5m,
                result.EscalationPercentage);

            Assert.Equal(
                "Annual",
                result.EscalationFrequency);

            Assert.False(
                string.IsNullOrWhiteSpace(result.RenewalTerms));

            Assert.False(
                string.IsNullOrWhiteSpace(result.TerminationTerms));
        }
        finally
        {
            File.Delete(documentPath);
        }
    }

    [Fact]
    public async Task ExtractAsync_ShouldProvideSourceTraceabilityForFields()
    {
        // Arrange
        var documentPath = CreateSampleLeaseFile();

        var documentExtractor = new StubDocumentExtractor();
        var agent = new StubLeaseAgent(documentExtractor);

        try
        {
            // Act
            var result = await agent.ExtractAsync(documentPath);

            // Assert
            Assert.NotEmpty(result.Fields);

            var unitField = result.Fields.First(
                field => field.FieldName == "UnitId");

            Assert.Equal(
                "MC-B-1204",
                unitField.Value);

            Assert.Equal(
                1,
                unitField.SourcePage);

            Assert.Contains(
                "Unit ID: MC-B-1204",
                unitField.SourceText);

            var monthlyRentField = result.Fields.First(
                field => field.FieldName == "MonthlyRent");

            Assert.Equal(
                "12000",
                monthlyRentField.Value);

            Assert.Equal(
                2,
                monthlyRentField.SourcePage);

            Assert.Contains(
                "Monthly Rent: QAR 12,000",
                monthlyRentField.SourceText);

            Assert.True(
                monthlyRentField.Confidence.HasValue);

            Assert.InRange(
                monthlyRentField.Confidence!.Value,
                0m,
                1m);
        }
        finally
        {
            File.Delete(documentPath);
        }
    }

    [Fact]
    public async Task ExtractAsync_ShouldNotCreateMissingFieldFlagsForCompleteLease()
    {
        // Arrange
        var documentPath = CreateSampleLeaseFile();

        var documentExtractor = new StubDocumentExtractor();
        var agent = new StubLeaseAgent(documentExtractor);

        try
        {
            // Act
            var result = await agent.ExtractAsync(documentPath);

            // Assert
            Assert.DoesNotContain(
                result.Flags,
                flag => flag.Type == "MissingField");
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
        var documentExtractor = new StubDocumentExtractor();
        var agent = new StubLeaseAgent(documentExtractor);

        var missingPath = Path.Combine(
            Path.GetTempPath(),
            $"missing-lease-{Guid.NewGuid()}.txt");

        // Act & Assert
        await Assert.ThrowsAsync<FileNotFoundException>(
            () => agent.ExtractAsync(missingPath));
    }
[Fact]
public async Task ExtractAsync_ShouldFlagMissingRequiredFields()
{
    // Arrange
    var documentPath = Path.Combine(
        Path.GetTempPath(),
        $"incomplete-lease-{Guid.NewGuid()}.txt");

    var content = """
        RESIDENTIAL LEASE AGREEMENT

        Page 1

        LANDLORD
        Marina Crest Holdings W.L.L.

        TENANT
        Ahmed Hassan


        Page 2

        RENT
        Annual Rent: QAR 144,000


        Page 3

        SIGNATURES

        Landlord Signature: Signed
        Tenant Signature: Signed
        """;

    await File.WriteAllTextAsync(
        documentPath,
        content);

    var documentExtractor = new StubDocumentExtractor();
    var agent = new StubLeaseAgent(documentExtractor);

    try
    {
        // Act
        var result = await agent.ExtractAsync(documentPath);

        // Assert
        Assert.Null(result.UnitId);
        Assert.Null(result.MonthlyRent);
        Assert.Null(result.DepositAmount);

        Assert.Contains(
            result.Flags,
            flag =>
                flag.Type == "MissingField" &&
                flag.Message.Contains(
                    "Unit ID",
                    StringComparison.OrdinalIgnoreCase));

        Assert.Contains(
            result.Flags,
            flag =>
                flag.Type == "MissingField" &&
                flag.Message.Contains(
                    "Monthly rent",
                    StringComparison.OrdinalIgnoreCase));

        Assert.Contains(
            result.Flags,
            flag =>
                flag.Type == "MissingField" &&
                flag.Message.Contains(
                    "Security deposit",
                    StringComparison.OrdinalIgnoreCase));
    }
    finally
    {
        File.Delete(documentPath);
    }
}
[Fact]
public async Task ExtractAsync_ShouldFlagContradictoryRentValues()
{
    // Arrange
    var documentPath = Path.Combine(
        Path.GetTempPath(),
        $"contradictory-lease-{Guid.NewGuid()}.txt");

    var content = """
        RESIDENTIAL LEASE AGREEMENT

        Page 1

        Unit ID: MC-B-1204
        Commencement Date: 01/01/2026
        Expiry Date: 31/12/2027
        Term: 24 months

        Page 2

        Monthly Rent: QAR 12,000
        Annual Rent: QAR 150,000
        Rent Frequency: Monthly
        Security Deposit: QAR 12,000

        Escalation
        Escalation Type: Percentage
        Escalation Percentage: 5%
        Escalation Frequency: Annual

        Page 3

        RENEWAL
        The lease may be renewed by mutual written agreement between the parties.

        TERMINATION
        Either party may terminate the lease by providing 60 days written notice.

        SIGNATURES
        Landlord Signature: Signed
        Tenant Signature: Signed
        """;

    await File.WriteAllTextAsync(
        documentPath,
        content);

    var documentExtractor = new StubDocumentExtractor();
    var agent = new StubLeaseAgent(documentExtractor);

    try
    {
        // Act
        var result = await agent.ExtractAsync(documentPath);

        // Assert
        Assert.Contains(
            result.Flags,
            flag =>
                flag.Type == "Contradiction" &&
                flag.Message.Contains(
                    "Annual rent",
                    StringComparison.OrdinalIgnoreCase));
    }
    finally
    {
        File.Delete(documentPath);
    }
}
[Fact]
public async Task ExtractAsync_ShouldFlagInvalidTermWhenDetected()
{
    // Arrange
    var documentPath = Path.Combine(
        Path.GetTempPath(),
        $"invalid-term-lease-{Guid.NewGuid()}.txt");

    var content = """
        RESIDENTIAL LEASE AGREEMENT

        Page 1

        Unit ID: MC-B-1204
        Commencement Date: 01/01/2026
        Expiry Date: 31/12/2027
        Term: 0 months

        Page 2

        Monthly Rent: QAR 12,000
        Annual Rent: QAR 144,000
        Rent Frequency: Monthly
        Security Deposit: QAR 12,000

        Page 3

        SIGNATURES
        Landlord Signature: Signed
        Tenant Signature: Signed
        """;

    await File.WriteAllTextAsync(
        documentPath,
        content);

    var documentExtractor = new StubDocumentExtractor();
    var agent = new StubLeaseAgent(documentExtractor);

    try
    {
        // Act
        var result = await agent.ExtractAsync(documentPath);

        // Assert
        Assert.Contains(
            result.Flags,
            flag =>
                flag.Type == "SuspiciousValue" &&
                flag.Message.Contains(
                    "Lease term",
                    StringComparison.OrdinalIgnoreCase));
    }
    finally
    {
        File.Delete(documentPath);
    }
}
    /// <summary>
    /// Creates a controlled fixture containing the information required
    /// by the stub lease agent. The test does not depend on the repository
    /// data folder, making it isolated and repeatable.
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

            TENANT
            Ahmed Hassan

            PROPERTY
            Marina Crest Residences
            Tower B
            Apartment 1204
            Unit ID: MC-B-1204

            LEASE TERM
            Commencement Date: 01/01/2026
            Expiry Date: 31/12/2027
            Term: 24 months


            Page 2

            RENT
            Monthly Rent: QAR 12,000
            Annual Rent: QAR 144,000
            Rent Frequency: Monthly

            SECURITY DEPOSIT
            Security Deposit: QAR 12,000

            ESCALATION
            The rent may be increased by 5% annually.
            Escalation Type: Percentage
            Escalation Percentage: 5%
            Escalation Frequency: Annual


            Page 3

            RENEWAL
            The lease may be renewed by mutual written agreement between the parties.

            TERMINATION
            Either party may terminate the lease by providing 60 days written notice.

            SIGNATURES

            Landlord: Marina Crest Holdings W.L.L.
            Landlord Signature: Signed

            Tenant: Ahmed Hassan
            Tenant Signature: Signed
            """;

        File.WriteAllText(path, content);

        return path;
    }
}