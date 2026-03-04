using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.DTO.Vehicles;
using Models.Models;
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

    // ===== DROPDOWN DATA ENDPOINTS =====

    [HttpGet("models")]
    [ProducesResponseType(typeof(List<VehicleModelDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<VehicleModelDto>>> GetVehicleModels()
    {
        using var context = new CarManagerContext();
        var models = await context.VehicleModels
            .Where(m => m.DeletedAt == null)
            .OrderBy(m => m.Manufacturer)
            .ThenBy(m => m.ModelName)
            .Select(m => new VehicleModelDto
            {
                Id = m.Id,
                Manufacturer = m.Manufacturer,
                ModelName = m.ModelName,
                Seats = m.Seats,
                EngineType = m.EngineType,
                DefaultPrice = m.DefaultPrice
            })
            .ToListAsync();

        return Ok(models);
    }

    [HttpGet("branches")]
    [ProducesResponseType(typeof(List<BranchDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<BranchDto>>> GetBranches()
    {
        using var context = new CarManagerContext();
        var branches = await context.Branches
            .Where(b => b.DeletedAt == null)
            .OrderBy(b => b.Name)
            .Select(b => new BranchDto
            {
                Id = b.Id,
                Name = b.Name,
                Address = b.Address
            })
            .ToListAsync();

        return Ok(branches);
    }

    [HttpGet("drivers")]
    [ProducesResponseType(typeof(List<DriverDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DriverDto>>> GetDrivers()
    {
        using var context = new CarManagerContext();
        var drivers = await context.Drivers
            .Where(d => d.DeletedAt == null && d.Status == "Active")
            .OrderBy(d => d.Name)
            .Select(d => new DriverDto
            {
                Id = d.Id,
                Name = d.Name,
                LicenseNumber = d.LicenseNumber,
                Phone = d.Phone
            })
            .ToListAsync();

        return Ok(drivers);
    }
}

// ===== DTOs for dropdown data =====

public class VehicleModelDto
{
    public int Id { get; set; }
    public string Manufacturer { get; set; }
    public string ModelName { get; set; }
    public int? Seats { get; set; }
    public string EngineType { get; set; }
    public decimal? DefaultPrice { get; set; }
}

public class BranchDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Address { get; set; }
}

public class DriverDto
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string LicenseNumber { get; set; }
    public string Phone { get; set; }
}

