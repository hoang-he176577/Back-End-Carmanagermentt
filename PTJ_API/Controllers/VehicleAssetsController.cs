using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Models.DTO.Vehicles;
using Service.Services.Interfaces;

namespace API.Controllers;

[ApiController]
[Route("api/assets/vehicles")]
public sealed class VehicleAssetsController : ControllerBase
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
    public async Task<ActionResult<VehicleAssetDto>> GetVehicleById([FromRoute] int id)
    {
        var vehicle = await _service.GetVehicleByIdAsync(id);

        if (vehicle == null)
        {
            return NotFound(new { message = "Vehicle not found." });
        }

        return Ok(vehicle);
    }

    [HttpPost]
    [ProducesResponseType(typeof(VehicleAssetDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VehicleAssetDto>> CreateVehicle([FromBody] VehicleCreateRequestDto request)
    {
        var result = await _service.CreateVehicleAsync(request);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        var created = result.Data!;
        return CreatedAtAction(nameof(GetVehicleById), new { id = created.Id }, created);
    }
}
