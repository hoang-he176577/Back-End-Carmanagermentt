using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.DTO.Accessories;
using Service.Services.Accessories.Interfaces;
using System.Security.Claims;

namespace API.Controllers.Accessories;

[ApiController]
[Authorize]
[Route("api/vehicle-accessories")]
public sealed class VehicleAccessoriesController : ControllerBase
{
    private readonly IAccessoryService _service;

    public VehicleAccessoriesController(IAccessoryService service)
    {
        _service = service;
    }

    [HttpPost("issue")]
    [Authorize(Roles = "Operator,Branch Asset Accountant,Manager,Executive Management")]
    [ProducesResponseType(typeof(IssueVehicleAccessoryResponseDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<IssueVehicleAccessoryResponseDto>> Issue([FromBody] IssueVehicleAccessoryRequestDto request)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var errorResult))
        {
            return errorResult!;
        }

        var result = await _service.IssueVehicleAccessoryAsync(actorUserId, roles, request);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return StatusCode(201, result.Data);
    }

    [HttpPost("{id:int}/return")]
    [Authorize(Roles = "Operator,Branch Asset Accountant,Manager,Executive Management")]
    [ProducesResponseType(typeof(VehicleAccessoryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<VehicleAccessoryDto>> ReturnOrDamageOrLost([FromRoute] int id, [FromBody] ReturnVehicleAccessoryRequestDto request)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var errorResult))
        {
            return errorResult!;
        }

        var result = await _service.HandleVehicleAccessoryActionAsync(actorUserId, roles, id, request);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return Ok(result.Data);
    }

    private bool TryGetActor(out int actorUserId, out IReadOnlyCollection<string> roles, out ActionResult? errorResult)
    {
        roles = User.FindAll(ClaimTypes.Role)
            .Select(claim => claim.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct()
            .ToList();

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? User.FindFirstValue("sub");

        if (!int.TryParse(userIdClaim, out actorUserId))
        {
            errorResult = Unauthorized(new { message = "Invalid user identity in token." });
            return false;
        }

        if (roles.Count == 0)
        {
            errorResult = StatusCode(403, new { message = "Role is required." });
            return false;
        }

        errorResult = null;
        return true;
    }
}
