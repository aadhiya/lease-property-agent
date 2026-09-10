using LeasePropertyAgent.Application.Interfaces;
using LeasePropertyAgent.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace LeasePropertyAgent.Api.Controllers;

[ApiController]
[Route("api/leases")]
public class LeaseController : ControllerBase
{
    private readonly ILeaseProcessingService _leaseProcessingService;

    public LeaseController(
        ILeaseProcessingService leaseProcessingService)
    {
        _leaseProcessingService = leaseProcessingService;
    }

    [HttpPost("process")]
    public async Task<IActionResult> ProcessLease(
        [FromBody] LeaseProcessingRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result =
                await _leaseProcessingService.ProcessAsync(
                    request.DocumentPath,
                    cancellationToken);

            var lease = result.Lease;

            var response = new
            {
                leaseId = lease.Id,
                unitId = lease.UnitId,
                documentName = lease.DocumentName,
                documentPath = lease.DocumentPath,
                status = lease.Status.ToString(),

                commencementDate = lease.CommencementDate,
                expiryDate = lease.ExpiryDate,
                termMonths = lease.TermMonths,

                monthlyRent = lease.MonthlyRent,
                annualRent = lease.AnnualRent,
                rentFrequency = lease.RentFrequency,
                currency = lease.Currency,
                depositAmount = lease.DepositAmount,

                escalationClause = lease.EscalationClause,
                renewalTerms = lease.RenewalTerms,
                terminationTerms = lease.TerminationTerms,

                parties = lease.Parties
                    .Select(party => new
                    {
                        id = party.Id,
                        role = party.Role.ToString(),
                        name = party.Name,
                        isPresent = party.IsPresent,
                        isSigned = party.IsSigned
                    })
                    .ToList(),

                fields = lease.Fields
                    .Select(field => new
                    {
                        id = field.Id,
                        fieldName = field.FieldName,
                        extractedValue = field.ExtractedValue,
                        reviewedValue = field.ReviewedValue,
                        confidence = field.Confidence,
                        sourcePage = field.SourcePage,
                        sourceText = field.SourceText,
                        reviewStatus = field.ReviewStatus.ToString()
                    })
                    .ToList(),

                flags = lease.Flags
                    .Select(flag => new
                    {
                        id = flag.Id,
                        type = flag.Type,
                        severity = flag.Severity.ToString(),
                        message = flag.Message,
                        sourcePage = flag.SourcePage,
                        sourceText = flag.SourceText,
                        status = flag.Status.ToString()
                    })
                    .ToList(),

                validationResults = lease.ValidationResults
                    .Select(validation => new
                    {
                        id = validation.Id,
                        validationRunId = validation.ValidationRunId,
                        ruleId = validation.RuleId,
                        status = validation.Status.ToString(),
                        reason = validation.Reason,
                        sourcePage = validation.SourcePage,
                        sourceText = validation.SourceText,
                        createdAt = validation.CreatedAt
                    })
                    .ToList()
            };

            return Ok(response);
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new
            {
                error = ex.Message
            });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new
            {
                error = ex.Message
            });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                error = ex.Message
            });
        }
    }
}