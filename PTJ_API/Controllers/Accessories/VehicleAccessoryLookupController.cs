using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.DTO.Accessories;
using Service.Services.Accessories.Interfaces;
using System.Security.Claims;

namespace API.Controllers.Accessories;

[ApiController]
[Authorize]
[Route("api/vehicles")]
public sealed class VehicleAccessoryLookupController : ControllerBase
{
    private readonly IAccessoryService _service;

    public VehicleAccessoryLookupController(IAccessoryService service)
    {
        _service = service;
    }

    [HttpGet("{vehicleId:int}/accessories")]
    [Authorize(Roles = "Operator,Branch Asset Accountant,Executive Management")]
    [ProducesResponseType(typeof(List<VehicleAccessoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<VehicleAccessoryDto>>> GetVehicleAccessories(
        [FromRoute] int vehicleId,
        [FromQuery] bool activeOnly = false)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var errorResult))
        {
            return errorResult!;
        }

        var result = await _service.GetVehicleAccessoriesAsync(actorUserId, roles, vehicleId, activeOnly);
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
