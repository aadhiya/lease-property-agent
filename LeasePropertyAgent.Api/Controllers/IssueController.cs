using LeasePropertyAgent.Application.Interfaces;
using LeasePropertyAgent.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace LeasePropertyAgent.Api.Controllers;

[ApiController]
[Route("api/issues")]
public class IssueController : ControllerBase
{
    private readonly IIssueProcessingService _issueProcessingService;

    public IssueController(IIssueProcessingService issueProcessingService)
    {
        _issueProcessingService = issueProcessingService;
    }

    [HttpPost("process")]
    public async Task<IActionResult> ProcessIssue(
        [FromBody] IssueProcessingRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _issueProcessingService.ProcessAsync(
                request,
                cancellationToken);

            var issue = result.Issue;

            var response = new IssueProcessingResponse
            {
                IssueId = issue.Id,
                UnitId = issue.UnitId,
                Title = issue.Title,
                Description = issue.Description,
                ConditionAssessment = issue.ConditionAssessment ?? string.Empty,
                Confidence = issue.Confidence,
                Status = issue.Status.ToString(),

                Images = issue.Images
                    .Select(image => new IssueImageResponse
                    {
                        Id = image.Id,
                        FileName = image.FileName,
                        FilePath = image.FilePath,
                        Observation = image.Observation,
                        Confidence = image.Confidence
                    })
                    .ToList(),

                WorkOrder = issue.WorkOrders
                    .Select(workOrder => new WorkOrderResponse
                    {
                        Id = workOrder.Id,
                        UnitId = workOrder.UnitId,
                        Title = workOrder.Title,
                        Description = workOrder.Description,
                        Status = workOrder.Status.ToString()
                    })
                    .FirstOrDefault()
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