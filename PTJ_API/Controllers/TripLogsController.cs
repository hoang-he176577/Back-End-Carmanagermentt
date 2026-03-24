using Microsoft.AspNetCore.Mvc;
using Models.DTO.Vehicles;
using Service.Services.VehicleAssets.Interfaces;

namespace API.Controllers;

[ApiController]
[Route("api/trips")]
public class TripLogsController : BaseController
{
    private readonly ITripLogService _tripService;

    public TripLogsController(ITripLogService tripService)
    {
        _tripService = tripService;
    }

    [HttpPost("start")]
    public async Task<IActionResult> StartTrip(StartTripRequestDto dto)
    {
        dto.OperatorId = GetUserId();
        return HandleCreated(await _tripService.StartTripAsync(dto), "Start trip successfully");
    }

    [HttpPut("{tripId:int}/end")]
    public async Task<IActionResult> EndTrip(int tripId, EndTripRequestDto dto)
    {
        dto.EndedBy = GetUserId();
        await _tripService.EndTripAsync(tripId, dto);
        return Ok("Trip ended successfully");
    }

    [HttpGet("history")]
    public async Task<IActionResult> GetAllHistory()
    {
        var result = await _tripService.GetAllTripHistoryAsync();
        return Ok(result);
    }

    [HttpGet("vehicle/{vehicleId:int}/history")]
    public async Task<IActionResult> GetVehicleTripHistory(int vehicleId)
    {
        var result = await _tripService.GetVehicleTripHistoryAsync(vehicleId);
        return Ok(result);
    }

    [HttpGet("vehicle/history")]
    public async Task<IActionResult> GetVehicleTripHistoryLegacy([FromQuery] int vehicleId)
        => await GetVehicleTripHistory(vehicleId);

    [HttpGet("manage/vehicles")]
    public async Task<IActionResult> GetManageVehicles([FromQuery] string? tab)
    {
        var branchId = GetBranchId();
        var result = await _tripService.GetManageVehiclesAsync(branchId, tab);
        return Ok(result);
    }

    [HttpGet("vehicles/dropdown")]
    public async Task<IActionResult> GetVehicleDrop()
    {
        var result = await _tripService.GetVehicleDropAsync();
        return Ok(result);
    }

    [HttpGet("vehicle/{vehicleId:int}/driver")]
    public async Task<IActionResult> GetDriverByVehicleId(int vehicleId)
        => HandleResult(await _tripService.GetDriverByVehicleIdAsync(vehicleId));
}
