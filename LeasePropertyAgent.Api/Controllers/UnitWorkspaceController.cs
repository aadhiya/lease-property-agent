using LeasePropertyAgent.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LeasePropertyAgent.Api.Controllers;

[ApiController]
[Route("api/units")]
public class UnitWorkspaceController : ControllerBase
{
    private readonly IUnitWorkspaceService _unitWorkspaceService;

    public UnitWorkspaceController(
        IUnitWorkspaceService unitWorkspaceService)
    {
        _unitWorkspaceService = unitWorkspaceService;
    }

    [HttpGet("{unitId:guid}/workspace")]
    public async Task<IActionResult> GetWorkspace(
        Guid unitId,
        CancellationToken cancellationToken)
    {
        var workspace = await _unitWorkspaceService.GetWorkspaceAsync(
            unitId,
            cancellationToken);

        if (workspace is null)
        {
            return NotFound(new
            {
                error = $"Unit '{unitId}' was not found."
            });
        }

        return Ok(workspace);
    }
}