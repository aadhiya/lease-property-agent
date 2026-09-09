using LeasePropertyAgent.Application.Interfaces;
using LeasePropertyAgent.Application.Models;
using Microsoft.AspNetCore.Mvc;

namespace LeasePropertyAgent.Api.Controllers;

[ApiController]
[Route("api/reviews")]
public class ReviewController : ControllerBase
{
    private readonly IReviewService _reviewService;

    public ReviewController(IReviewService reviewService)
    {
        _reviewService = reviewService;
    }

    [HttpPost("lease-fields/{fieldId:guid}")]
    public async Task<IActionResult> ReviewLeaseField(
        Guid fieldId,
        [FromBody] ReviewLeaseFieldRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _reviewService.ReviewLeaseFieldAsync(
                fieldId,
                request,
                cancellationToken);

            return NoContent();
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPost("lease-flags/{flagId:guid}")]
    public async Task<IActionResult> ReviewLeaseFlag(
        Guid flagId,
        [FromBody] ReviewLeaseFlagRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _reviewService.ReviewLeaseFlagAsync(
                flagId,
                request,
                cancellationToken);

            return NoContent();
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }

    [HttpPost("work-orders/{workOrderId:guid}")]
    public async Task<IActionResult> ReviewWorkOrder(
        Guid workOrderId,
        [FromBody] ReviewWorkOrderRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            await _reviewService.ReviewWorkOrderAsync(
                workOrderId,
                request,
                cancellationToken);

            return NoContent();
        }
        catch (ArgumentNullException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { error = ex.Message });
        }
    }
}