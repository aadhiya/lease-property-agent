using LeasePropertyAgent.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LeasePropertyAgent.Api.Controllers;

[ApiController]
[Route("api/units")]
public class UnitController : ControllerBase
{
    private readonly IUnitService _unitService;

    public UnitController(IUnitService unitService)
    {
        _unitService = unitService;
    }

    [HttpGet]
    public async Task<IActionResult> GetUnits(
        CancellationToken cancellationToken)
    {
        var units = await _unitService.GetAllAsync(
            cancellationToken);

        return Ok(units);
    }
}