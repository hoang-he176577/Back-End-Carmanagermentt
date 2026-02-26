using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.DTO.Vehicles;
using Service.Services.Interfaces;

namespace API.Controllers;

[ApiController]
[Route("api/assets/vehicles")]
public sealed class VehicleAssetsController : BaseController
{
    private readonly IVehicleAssetService _service;

    public VehicleAssetsController(IVehicleAssetService service)
    {
        _service = service;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<VehicleAssetDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<VehicleAssetDto>>> GetVehicles(
        [FromQuery] int? branchId,
        [FromQuery] string? status,
        [FromQuery] bool includeDeleted = false)
    {
        var vehicles = await _service.GetVehiclesAsync(branchId, status, includeDeleted);
        return Ok(vehicles);
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
    public async Task<IActionResult> GetVehicleById([FromRoute] int id)
    {
        var vehicle = await _service.GetVehicleByIdAsync(id);
        if (vehicle == null)
        {
            return NotFound(new { message = "Vehicle not found." });
        }

        return Ok(vehicle);
    }

    // ===== SIMPLE CREATE (demo) =====

    [HttpPost]
    [ProducesResponseType(typeof(VehicleAssetDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateVehicle([FromBody] VehicleCreateRequestDto request)
    {
        var result = await _service.CreateVehicleAsync(request);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        var created = result.Data!;
        return CreatedAtAction(nameof(GetVehicleById), new { id = created.Id }, created);
    }

    // ===== ASSET MANAGEMENT (CREATE) - ONLY ACCOUNTANT =====

    [HttpPost("asset-create")]
    [Authorize(Roles = "Branch Asset Accountant")]
    [ProducesResponseType(typeof(VehicleAssetDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateAsset([FromBody] VehicleAssetCreateRequestDto request)
    {
        var accountantUserId = GetUserId();
        var result = await _service.CreateAssetAsync(request, accountantUserId);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        var created = result.Data!;
        return CreatedAtAction(nameof(GetVehicleById), new { id = created.Id }, created);
    }

    // ===== VEHICLE ASSIGNMENT =====

    [HttpPost("{id:int}/assign")]
    [Authorize]
    [ProducesResponseType(typeof(VehicleAssetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> AssignVehicle([FromRoute] int id, [FromBody] VehicleAssignRequestDto request)
    {
        var userId = GetUserId();
        var result = await _service.AssignVehicleAsync(id, request, userId);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return HandleResult(result.Data);
    }

    [HttpPost("{id:int}/unassign")]
    [Authorize]
    [ProducesResponseType(typeof(VehicleAssetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnassignVehicle([FromRoute] int id, [FromBody] VehicleUnassignRequestDto request)
    {
        var userId = GetUserId();
        // Bảo đảm DTO có vehicleId nhất quán nếu được gửi lên
        request.VehicleId = id;

        var result = await _service.UnassignVehicleAsync(id, request, userId);

        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return HandleResult(result.Data);
    }
}

