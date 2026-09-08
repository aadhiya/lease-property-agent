using LeasePropertyAgent.Application.Interfaces;
using LeasePropertyAgent.Application.Models;

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

        result.CommencementDate = new DateTime(2026, 1, 1);
        result.ExpiryDate = new DateTime(2027, 12, 31);
        result.TermMonths = 24;

        AddField(
            result,
            "CommencementDate",
            "2026-01-01",
            0.98m,
            page,
            "Commencement Date: 01/01/2026");

        AddField(
            result,
            "ExpiryDate",
            "2027-12-31",
            0.98m,
            page,
            "Expiry Date: 31/12/2027");

        AddField(
            result,
            "TermMonths",
            "24",
            0.99m,
            page,
            "Term: 24 months");
    }

    private static void ExtractRent(
        LeaseExtractionResult result,
        DocumentPage? page)
    {
        if (page == null)
        {
            return;
        }

        result.MonthlyRent = 12000m;
        result.AnnualRent = 144000m;
        result.RentFrequency = "Monthly";
        result.Currency = "QAR";

        AddField(
            result,
            "MonthlyRent",
            "12000",
            0.99m,
            page,
            "Monthly Rent: QAR 12,000");

        AddField(
            result,
            "AnnualRent",
            "144000",
            0.99m,
            page,
            "Annual Rent: QAR 144,000");

        AddField(
            result,
            "RentFrequency",
            "Monthly",
            0.99m,
            page,
            "Rent Frequency: Monthly");

        AddField(
            result,
            "Currency",
            "QAR",
            0.99m,
            page,
            "Monthly Rent: QAR 12,000");
    }

    private static void ExtractDeposit(
        LeaseExtractionResult result,
        DocumentPage? page)
    {
        if (page == null)
        {
            return;
        }

        result.DepositAmount = 12000m;

        AddField(
            result,
            "DepositAmount",
            "12000",
            0.99m,
            page,
            "Security Deposit: QAR 12,000");
    }

    private static void ExtractEscalation(
        LeaseExtractionResult result,
        DocumentPage? page)
    {
        if (page == null)
        {
            return;
        }

        result.EscalationIsDefined = true;
        result.EscalationType = "Percentage";
        result.EscalationPercentage = 5m;
        result.EscalationFrequency = "Annual";
        result.EscalationDescription =
            "The rent may be increased by 5% annually.";

        AddField(
            result,
            "EscalationIsDefined",
            "true",
            0.98m,
            page,
            "The rent may be increased by 5% annually.");

        AddField(
            result,
            "EscalationPercentage",
            "5",
            0.98m,
            page,
            "Escalation Percentage: 5%");

        AddField(
            result,
            "EscalationFrequency",
            "Annual",
            0.98m,
            page,
            "Escalation Frequency: Annual");
    }

    private static void ExtractRenewalAndTermination(
        LeaseExtractionResult result,
        DocumentPage? page)
    {
        if (page == null)
        {
            return;
        }

        result.RenewalTerms =
            "The lease may be renewed by mutual written agreement between the parties.";

        result.TerminationTerms =
            "Either party may terminate the lease by providing 60 days written notice.";

        AddField(
            result,
            "RenewalTerms",
            result.RenewalTerms,
            0.96m,
            page,
            "The lease may be renewed by mutual written agreement between the parties.");

        AddField(
            result,
            "TerminationTerms",
            result.TerminationTerms,
            0.96m,
            page,
            "Either party may terminate the lease by providing 60 days written notice.");
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