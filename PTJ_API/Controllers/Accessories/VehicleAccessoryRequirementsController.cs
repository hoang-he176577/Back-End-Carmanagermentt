using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.DTO.Accessories;
using Service.Services.Accessories.Interfaces;
using System.Security.Claims;

namespace API.Controllers.Accessories;

[ApiController]
[Authorize]
[Route("api/vehicle-accessory-requirements")]
public sealed class VehicleAccessoryRequirementsController : ControllerBase
{
    private readonly IAccessoryService _service;

    public VehicleAccessoryRequirementsController(IAccessoryService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = "Operator,Branch Asset Accountant,Manager,Executive Management")]
    [ProducesResponseType(typeof(List<VehicleAccessoryRequirementDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<VehicleAccessoryRequirementDto>>> GetList([FromQuery] int? modelId)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var errorResult))
        {
            return errorResult!;
        }

        var result = await _service.GetVehicleAccessoryRequirementsAsync(actorUserId, roles, modelId);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return Ok(result.Data);
    }

    [HttpGet("check/vehicle/{vehicleId:int}")]
    [Authorize(Roles = "Operator,Branch Asset Accountant,Manager,Executive Management")]
    [ProducesResponseType(typeof(VehicleAccessoryRequirementCheckResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<VehicleAccessoryRequirementCheckResultDto>> Check([FromRoute] int vehicleId)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var errorResult))
        {
            return errorResult!;
        }

        var result = await _service.CheckVehicleAccessoryRequirementsAsync(actorUserId, roles, vehicleId);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return Ok(result.Data);
    }

    [HttpPost]
    [Authorize(Roles = "Branch Asset Accountant,Manager,Executive Management")]
    [ProducesResponseType(typeof(VehicleAccessoryRequirementDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<VehicleAccessoryRequirementDto>> Create([FromBody] VehicleAccessoryRequirementUpsertRequestDto request)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var errorResult))
        {
            return errorResult!;
        }

        var result = await _service.CreateVehicleAccessoryRequirementAsync(actorUserId, roles, request);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return StatusCode(StatusCodes.Status201Created, result.Data);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Branch Asset Accountant,Manager,Executive Management")]
    [ProducesResponseType(typeof(VehicleAccessoryRequirementDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<VehicleAccessoryRequirementDto>> Update([FromRoute] int id, [FromBody] VehicleAccessoryRequirementUpsertRequestDto request)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var errorResult))
        {
            return errorResult!;
        }

        var result = await _service.UpdateVehicleAccessoryRequirementAsync(actorUserId, roles, id, request);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return Ok(result.Data);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Branch Asset Accountant,Manager,Executive Management")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var errorResult))
        {
            return errorResult!;
        }

        var result = await _service.DeleteVehicleAccessoryRequirementAsync(actorUserId, roles, id);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return NoContent();
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
