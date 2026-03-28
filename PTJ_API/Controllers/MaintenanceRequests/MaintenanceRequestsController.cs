using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.DTO.Maintenance;
using Service.Services.MaintenanceRequests.Interfaces;

namespace API.Controllers.MaintenanceRequests;

[ApiController]
[Authorize]
[Route("api/maintenance-requests")]
public sealed class MaintenanceRequestsController : ControllerBase
{
    private readonly IMaintenanceRequestService _service;

    public MaintenanceRequestsController(IMaintenanceRequestService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<MaintenanceRequestDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<MaintenanceRequestDto>>> GetList(
        [FromQuery] string? status,
        [FromQuery] string? maintenanceType,
        [FromQuery] int? vehicleId,
        [FromQuery] bool includeDeleted = false)
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        int.TryParse(userIdClaim, out var userId);
        var userRole = User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;

        var result = await _service.GetListAsync(status, maintenanceType, includeDeleted, userId, userRole, vehicleId);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return Ok(result.Data);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(MaintenanceRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<MaintenanceRequestDto>> GetById([FromRoute] int id, [FromQuery] bool includeDeleted = false)
    {
        var result = await _service.GetByIdAsync(id, includeDeleted);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return Ok(result.Data);
    }

    [HttpPost]
    [Authorize(Roles = "Operator")]
    [ProducesResponseType(typeof(MaintenanceRequestDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<MaintenanceRequestDto>> Create([FromBody] MaintenanceCreateRequestDto request)
    {
        var actorClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? User.FindFirstValue("sub");

        if (!int.TryParse(actorClaim, out var actorUserId))
        {
            return Unauthorized(new { message = "Invalid user identity in token." });
        }

        var result = await _service.CreateAsync(actorUserId, request);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        var created = result.Data!;
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Operator")]
    [ProducesResponseType(typeof(MaintenanceRequestDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<MaintenanceRequestDto>> Update([FromRoute] int id, [FromBody] MaintenanceUpdateRequestDto request)
    {
        var result = await _service.UpdateAsync(id, request);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return Ok(result.Data);
    }

    [HttpPatch("{id:int}/approval")]
    [Authorize(Roles = "Executive Management,Manager")]
    [ProducesResponseType(typeof(MaintenanceRequestDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<MaintenanceRequestDto>> ApproveOrReject([FromRoute] int id, [FromBody] MaintenanceApprovalRequestDto request)
    {
        var actorClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? User.FindFirstValue("sub");

        if (!int.TryParse(actorClaim, out var approverUserId))
        {
            return Unauthorized(new { message = "Invalid user identity in token." });
        }

        var result = await _service.ApproveOrRejectAsync(id, approverUserId, request);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return Ok(result.Data);
    }

    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        var result = await _service.DeleteAsync(id);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return Ok(new { message = "Deleted successfully." });
    }
}
