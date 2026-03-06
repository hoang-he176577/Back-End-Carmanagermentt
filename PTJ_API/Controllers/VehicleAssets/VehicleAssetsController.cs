using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.DTO.Vehicles;
using Models.Models;
using Service.Services.VehicleAssets.Interfaces;

namespace API.Controllers.VehicleAssets;

[ApiController]
[Authorize]

[Route("api/assets/vehicles")]
public sealed class VehicleAssetsController : ControllerBase
{
    private readonly IVehicleAssetService _service;

    public VehicleAssetsController(IVehicleAssetService service)
    {
        _service = service;
    }

    // ───────────────── Dropdown Data ─────────────────

    [HttpGet("models")]
    [ProducesResponseType(typeof(List<VehicleModelDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetModels()
    {
        var result = await _service.GetVehicleModelsAsync();
        return Ok(result);
    }

    [HttpGet("drivers")]
    [ProducesResponseType(typeof(List<DriverDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDrivers()
    {
        var result = await _service.GetDriversAsync();
        return Ok(result);
    }

    [HttpGet("branches")]
    [ProducesResponseType(typeof(List<BranchDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBranches()
    {
        var result = await _service.GetBranchesAsync();
        return Ok(result);
    }

    // ───────────────── Original Endpoints ─────────────────

    [HttpGet]
    [ProducesResponseType(typeof(List<VehicleAssetDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<VehicleAssetDto>>> GetVehicles(
        [FromQuery] int? branchId,
        [FromQuery] string? status,
        [FromQuery] bool includeDeleted = false)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var errorResult))
        {
            return errorResult!;
        }

        var result = await _service.GetVehiclesAsync(actorUserId, roles, branchId, status, includeDeleted);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return Ok(result.Data);
    }

    [HttpGet("branches/{branchId:int}")]
    [ProducesResponseType(typeof(List<VehicleAssetDto>), StatusCodes.Status200OK)]
    public Task<ActionResult<List<VehicleAssetDto>>> GetVehiclesByBranch(
        [FromRoute] int branchId,
        [FromQuery] string? status,
        [FromQuery] bool includeDeleted = false)
    {
        return GetVehicles(branchId, status, includeDeleted);
    }

    [HttpGet("/api/vehicles")]
    [ProducesResponseType(typeof(List<VehicleAssetDto>), StatusCodes.Status200OK)]
    public Task<ActionResult<List<VehicleAssetDto>>> GetVehiclesList(
        [FromQuery] int? branchId,
        [FromQuery] string? status,
        [FromQuery] bool includeDeleted = false)
    {
        return GetVehicles(branchId, status, includeDeleted);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(VehicleAssetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VehicleAssetDto>> GetVehicleById([FromRoute] int id)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var errorResult))
        {
            return errorResult!;
        }

        var result = await _service.GetVehicleByIdAsync(actorUserId, roles, id);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return Ok(result.Data);
    }

    [HttpPost]
    [ProducesResponseType(typeof(VehicleAssetDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VehicleAssetDto>> CreateVehicle([FromBody] VehicleCreateRequestDto request)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var errorResult))
        {
            return errorResult!;
        }

        var result = await _service.CreateVehicleAsync(actorUserId, roles, request);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        var created = result.Data!;
        return CreatedAtAction(nameof(GetVehicleById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(VehicleAssetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VehicleAssetDto>> UpdateVehicle([FromRoute] int id, [FromBody] VehicleUpdateRequestDto request)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var errorResult))
        {
            return errorResult!;
        }

        var result = await _service.UpdateVehicleAsync(actorUserId, roles, id, request);
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
