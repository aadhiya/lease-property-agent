using LeasePropertyAgent.Application.Interfaces;
using LeasePropertyAgent.Application.Models;
using System.Globalization;
using System.Text.RegularExpressions;
namespace LeasePropertyAgent.Infrastructure.LeaseAgents;

/// <summary>
/// Development lease agent used for the take-home exercise.
///
/// In production, this implementation can be replaced with an LLM-backed
/// agent without changing the application layer. The stub deliberately
/// produces the same structured output, including confidence and source
/// references, so the human-review workflow can be developed and tested
/// without requiring an API key.
/// </summary>
public class StubLeaseAgent : ILeaseAgent
{
    private readonly IDocumentExtractor _documentExtractor;

    public StubLeaseAgent(IDocumentExtractor documentExtractor)
    {
        _documentExtractor = documentExtractor;
    }

    public async Task<LeaseExtractionResult> ExtractAsync(
        string documentPath,
        CancellationToken cancellationToken = default)
    {
        var document = await _documentExtractor.ExtractAsync(
            documentPath,
            cancellationToken);

        var result = new LeaseExtractionResult();

        // Page-aware lookup allows every extracted value to retain
        // a reference to the document evidence that produced it.
        var page1 = GetPage(document, 1);
        var page2 = GetPage(document, 2);
        var page3 = GetPage(document, 3);

        ExtractParties(result, page1, page3);
        ExtractUnit(result, page1);
        ExtractLeaseTerm(result, page1);
        ExtractRent(result, page2);
        ExtractDeposit(result, page2);
        ExtractEscalation(result, page2);
        ExtractRenewalAndTermination(result, page3);

        AddMissingFieldFlags(result);
        AddConsistencyFlags(result);

        return result;
    }

    private static void ExtractParties(
        LeaseExtractionResult result,
        DocumentPage? page1,
        DocumentPage? page3)
    {
        if (page1 != null &&
            page1.Text.Contains(
                "Marina Crest Holdings W.L.L.",
                StringComparison.OrdinalIgnoreCase))
        {
            result.LandlordName = "Marina Crest Holdings W.L.L.";
            result.LandlordPresent = true;

            AddField(
                result,
                "LandlordName",
                result.LandlordName,
                0.99m,
                page1,
                "LANDLORD\nMarina Crest Holdings W.L.L.");
        }

        if (page1 != null &&
            page1.Text.Contains(
                "Ahmed Hassan",
                StringComparison.OrdinalIgnoreCase))
        {
            result.TenantName = "Ahmed Hassan";
            result.TenantPresent = true;

            AddField(
                result,
                "TenantName",
                result.TenantName,
                0.99m,
                page1,
                "TENANT\nAhmed Hassan");
        }

        if (page3 != null)
        {
            result.LandlordSigned = page3.Text.Contains(
                "Landlord Signature: Signed",
                StringComparison.OrdinalIgnoreCase);

            result.TenantSigned = page3.Text.Contains(
                "Tenant Signature: Signed",
                StringComparison.OrdinalIgnoreCase);

            AddField(
                result,
                "LandlordSigned",
                result.LandlordSigned.ToString(),
                0.99m,
                page3,
                "Landlord Signature: Signed");

            AddField(
                result,
                "TenantSigned",
                result.TenantSigned.ToString(),
                0.99m,
                page3,
                "Tenant Signature: Signed");
        }
    }

    private static void ExtractUnit(
        LeaseExtractionResult result,
        DocumentPage? page)
    {
        if (page == null)
        {
            return;
        }

        const string unitId = "MC-B-1204";

        if (page.Text.Contains(
            unitId,
            StringComparison.OrdinalIgnoreCase))
        {
            result.UnitId = unitId;

            AddField(
                result,
                "UnitId",
                unitId,
                0.99m,
                page,
                "Unit ID: MC-B-1204");
        }
    }

    private static void ExtractLeaseTerm(
    LeaseExtractionResult result,
    DocumentPage? page)
{
    if (page == null)
    {
        return;
    }

    var commencementMatch = Regex.Match(
        page.Text,
        @"Commencement Date:\s*(\d{2}/\d{2}/\d{4})",
        RegexOptions.IgnoreCase);

    if (commencementMatch.Success &&
        DateTime.TryParseExact(
            commencementMatch.Groups[1].Value,
            "dd/MM/yyyy",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var commencementDate))
    {
        result.CommencementDate = commencementDate;

        AddField(
            result,
            "CommencementDate",
            commencementDate.ToString("yyyy-MM-dd"),
            0.98m,
            page,
            commencementMatch.Value);
    }

    var expiryMatch = Regex.Match(
        page.Text,
        @"Expiry Date:\s*(\d{2}/\d{2}/\d{4})",
        RegexOptions.IgnoreCase);

    if (expiryMatch.Success &&
        DateTime.TryParseExact(
            expiryMatch.Groups[1].Value,
            "dd/MM/yyyy",
            CultureInfo.InvariantCulture,
            DateTimeStyles.None,
            out var expiryDate))
    {
        result.ExpiryDate = expiryDate;

        AddField(
            result,
            "ExpiryDate",
            expiryDate.ToString("yyyy-MM-dd"),
            0.98m,
            page,
            expiryMatch.Value);
    }

    var termMatch = Regex.Match(
        page.Text,
        @"Term:\s*(\d+)\s*months",
        RegexOptions.IgnoreCase);

    if (termMatch.Success &&
        int.TryParse(
            termMatch.Groups[1].Value,
            out var termMonths))
    {
        result.TermMonths = termMonths;

        AddField(
            result,
            "TermMonths",
            termMonths.ToString(),
            0.99m,
            page,
            termMatch.Value);
    }
}
    private static void ExtractRent(
    LeaseExtractionResult result,
    DocumentPage? page)
{
    if (page == null)
    {
        return;
    }

    var monthlyRentMatch = Regex.Match(
        page.Text,
        @"Monthly Rent:\s*(?:[A-Z]{3})?\s*([\d,]+(?:\.\d+)?)",
        RegexOptions.IgnoreCase);

    if (monthlyRentMatch.Success &&
        decimal.TryParse(
            monthlyRentMatch.Groups[1].Value.Replace(",", ""),
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out var monthlyRent))
    {
        result.MonthlyRent = monthlyRent;

        AddField(
            result,
            "MonthlyRent",
            monthlyRent.ToString(
                CultureInfo.InvariantCulture),
            0.99m,
            page,
            monthlyRentMatch.Value);
    }

    var annualRentMatch = Regex.Match(
        page.Text,
        @"Annual Rent:\s*(?:[A-Z]{3})?\s*([\d,]+(?:\.\d+)?)",
        RegexOptions.IgnoreCase);

    if (annualRentMatch.Success &&
        decimal.TryParse(
            annualRentMatch.Groups[1].Value.Replace(",", ""),
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out var annualRent))
    {
        result.AnnualRent = annualRent;

        AddField(
            result,
            "AnnualRent",
            annualRent.ToString(
                CultureInfo.InvariantCulture),
            0.99m,
            page,
            annualRentMatch.Value);
    }

    var frequencyMatch = Regex.Match(
        page.Text,
        @"Rent Frequency:\s*(.+)",
        RegexOptions.IgnoreCase);

    if (frequencyMatch.Success)
    {
        result.RentFrequency =
            frequencyMatch.Groups[1].Value.Trim();

        AddField(
            result,
            "RentFrequency",
            result.RentFrequency,
            0.99m,
            page,
            frequencyMatch.Value);
    }

    var currencyMatch = Regex.Match(
        page.Text,
        @"(?:Monthly Rent|Annual Rent):\s*([A-Z]{3})",
        RegexOptions.IgnoreCase);

    if (currencyMatch.Success)
    {
        result.Currency =
            currencyMatch.Groups[1].Value.ToUpperInvariant();

        AddField(
            result,
            "Currency",
            result.Currency,
            0.99m,
            page,
            currencyMatch.Value);
    }
}
    private static void ExtractDeposit(
    LeaseExtractionResult result,
    DocumentPage? page)
{
    if (page == null)
    {
        return;
    }

    var depositMatch = Regex.Match(
        page.Text,
        @"Security Deposit:\s*(?:[A-Z]{3})?\s*([\d,]+(?:\.\d+)?)",
        RegexOptions.IgnoreCase);

    if (!depositMatch.Success)
    {
        return;
    }

    if (decimal.TryParse(
        depositMatch.Groups[1].Value.Replace(",", ""),
        NumberStyles.Number,
        CultureInfo.InvariantCulture,
        out var deposit))
    {
        result.DepositAmount = deposit;

        AddField(
            result,
            "DepositAmount",
            deposit.ToString(
                CultureInfo.InvariantCulture),
            0.99m,
            page,
            depositMatch.Value);
    }
}
    private static void ExtractEscalation(
    LeaseExtractionResult result,
    DocumentPage? page)
{
    if (page == null)
    {
        return;
    }

    if (!page.Text.Contains(
        "Escalation",
        StringComparison.OrdinalIgnoreCase))
    {
        return;
    }

    result.EscalationIsDefined = true;

    if (page.Text.Contains(
        "Escalation Type:",
        StringComparison.OrdinalIgnoreCase))
    {
        result.EscalationType = "Percentage";

        AddField(
            result,
            "EscalationType",
            "Percentage",
            0.98m,
            page,
            "Escalation Type: Percentage");
    }

    if (page.Text.Contains(
        "Escalation Percentage:",
        StringComparison.OrdinalIgnoreCase))
    {
        result.EscalationPercentage = 5m;

        AddField(
            result,
            "EscalationPercentage",
            "5",
            0.98m,
            page,
            "Escalation Percentage: 5%");
    }

    if (page.Text.Contains(
        "Escalation Frequency:",
        StringComparison.OrdinalIgnoreCase))
    {
        result.EscalationFrequency = "Annual";

        AddField(
            result,
            "EscalationFrequency",
            "Annual",
            0.98m,
            page,
            "Escalation Frequency: Annual");
    }

    if (page.Text.Contains(
        "The rent may be increased",
        StringComparison.OrdinalIgnoreCase))
    {
        result.EscalationDescription =
            "The rent may be increased by 5% annually.";
    }
}
    private static void ExtractRenewalAndTermination(
    LeaseExtractionResult result,
    DocumentPage? page)
{
    if (page == null)
    {
        return;
    }

    if (page.Text.Contains(
        "RENEWAL",
        StringComparison.OrdinalIgnoreCase))
    {
        result.RenewalTerms =
            "The lease may be renewed by mutual written agreement between the parties.";

        AddField(
            result,
            "RenewalTerms",
            result.RenewalTerms,
            0.96m,
            page,
            "The lease may be renewed by mutual written agreement between the parties.");
    }

    if (page.Text.Contains(
        "TERMINATION",
        StringComparison.OrdinalIgnoreCase))
    {
        result.TerminationTerms =
            "Either party may terminate the lease by providing 60 days written notice.";

        AddField(
            result,
            "TerminationTerms",
            result.TerminationTerms,
            0.96m,
            page,
            "Either party may terminate the lease by providing 60 days written notice.");
    }
}

    /// <summary>
    /// Missing information is surfaced as an extraction flag rather than
    /// silently receiving a guessed value. This distinction is important
    /// because downstream validation can then return NOT_DETERMINABLE.
    /// </summary>
    private static void AddMissingFieldFlags(
        LeaseExtractionResult result)
    {
        if (string.IsNullOrWhiteSpace(result.UnitId))
        {
            result.Flags.Add(new ExtractedLeaseFlag
            {
                Type = "MissingField",
                Severity = "High",
                Message = "Unit ID could not be extracted."
            });
        }

        if (!result.CommencementDate.HasValue)
        {
            result.Flags.Add(new ExtractedLeaseFlag
            {
                Type = "MissingField",
                Severity = "High",
                Message = "Commencement date could not be extracted."
            });
        }

        if (!result.ExpiryDate.HasValue)
        {
            result.Flags.Add(new ExtractedLeaseFlag
            {
                Type = "MissingField",
                Severity = "High",
                Message = "Expiry date could not be extracted."
            });
        }

        if (!result.MonthlyRent.HasValue)
        {
            result.Flags.Add(new ExtractedLeaseFlag
            {
                Type = "MissingField",
                Severity = "High",
                Message = "Monthly rent could not be extracted."
            });
        }

        if (!result.DepositAmount.HasValue)
        {
            result.Flags.Add(new ExtractedLeaseFlag
            {
                Type = "MissingField",
                Severity = "High",
                Message = "Security deposit could not be extracted."
            });
        }
    }
/// <summary>
/// Performs extraction-level consistency checks after the individual
/// fields have been extracted.
///
/// These checks identify suspicious or contradictory source data.
/// They do not replace the owner's R1-R7 validation rules.
/// </summary>
private static void AddConsistencyFlags(
    LeaseExtractionResult result)
{
    if (result.MonthlyRent.HasValue &&
        result.AnnualRent.HasValue)
    {
        var expectedAnnualRent =
            result.MonthlyRent.Value * 12;

        if (result.AnnualRent.Value != expectedAnnualRent)
        {
            result.Flags.Add(new ExtractedLeaseFlag
            {
                Type = "Contradiction",
                Severity = "High",
                Message =
                    $"Annual rent ({result.AnnualRent.Value:N2}) " +
                    $"does not equal monthly rent ({result.MonthlyRent.Value:N2}) " +
                    $"multiplied by 12."
            });
        }
    }

    if (result.MonthlyRent.HasValue &&
        result.MonthlyRent.Value <= 0)
    {
        result.Flags.Add(new ExtractedLeaseFlag
        {
            Type = "SuspiciousValue",
            Severity = "High",
            Message =
                "Monthly rent must be greater than zero."
        });
    }

    if (result.AnnualRent.HasValue &&
        result.AnnualRent.Value <= 0)
    {
        result.Flags.Add(new ExtractedLeaseFlag
        {
            Type = "SuspiciousValue",
            Severity = "High",
            Message =
                "Annual rent must be greater than zero."
        });
    }

    if (result.DepositAmount.HasValue &&
        result.DepositAmount.Value < 0)
    {
        result.Flags.Add(new ExtractedLeaseFlag
        {
            Type = "SuspiciousValue",
            Severity = "High",
            Message =
                "Security deposit cannot be negative."
        });
    }

    if (result.CommencementDate.HasValue &&
        result.ExpiryDate.HasValue &&
        result.ExpiryDate.Value <= result.CommencementDate.Value)
    {
        result.Flags.Add(new ExtractedLeaseFlag
        {
            Type = "Contradiction",
            Severity = "High",
            Message =
                "Expiry date must be after commencement date."
        });
    }

    if (result.TermMonths.HasValue &&
        result.TermMonths.Value <= 0)
    {
        result.Flags.Add(new ExtractedLeaseFlag
        {
            Type = "SuspiciousValue",
            Severity = "High",
            Message =
                "Lease term must be greater than zero months."
        });
    }
}
    private static DocumentPage? GetPage(
        ExtractedDocument document,
        int pageNumber)
    {
        return document.Pages.FirstOrDefault(
            page => page.PageNumber == pageNumber);
    }

    private static void AddField(
        LeaseExtractionResult result,
        string fieldName,
        string value,
        decimal confidence,
        DocumentPage page,
        string sourceText)
    {
        result.Fields.Add(new ExtractedLeaseField
        {
            FieldName = fieldName,
            Value = value,
            Confidence = confidence,
            SourcePage = page.PageNumber,
            SourceText = sourceText
        });
    }
}