using Microsoft.AspNetCore.Mvc;

using Models.DTO.Vehicles;
using Service.Services.VehicleAssets.Interfaces;

namespace API.Controllers;

[ApiController]
[Route("api/trips")]
public class TripLogController : BaseController
{
    private readonly ITripLogsService _service;

    public TripLogController(ITripLogsService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _service.GetAllAsync();
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _service.GetByIdAsync(id);

        if (result == null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartTrip(StartTripRequestDto request)
    {
        request.OperatorId = GetUserId();
        var result = await _service.StartTripAsync(request);
        return Ok(result);
    }

    [HttpPut("{id}/end")]
    public async Task<IActionResult> EndTrip(int id, EndTripRequestDto request)
    {
        var result = await _service.EndTripAsync(id, request);
        return Ok(result);
    }

    [HttpPut("end/{id}")]
    public async Task<IActionResult> EndTripLegacy(int id, EndTripRequestDto request)
        => await EndTrip(id, request);

    [HttpPost("end/{id}")]
    public async Task<IActionResult> EndTripLegacyPost(int id, EndTripRequestDto request)
        => await EndTrip(id, request);

    [HttpGet("manage/vehicles")]
    public async Task<IActionResult> GetManageVehicles([FromQuery] string? tab)
    {
        var branchId = GetBranchId();
        var result = await _service.GetManageVehiclesAsync(branchId, tab);
        return Ok(result);
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetAllHistory()
    {
        var result = await _service.GetAllTripHistoryAsync();
        return Ok(result);
    }

    [HttpGet("vehicle/{vehicleId}/history")]
    public async Task<IActionResult> GetVehicleHistory(int vehicleId)
    {
        var result = await _service.GetVehicleTripHistoryAsync(vehicleId);
        return Ok(result);
    }
}
