using Microsoft.AspNetCore.Mvc;

using Models.DTO.Vehicles;
using Service.Services.VehicleAssets.Interfaces;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TripLogController : ControllerBase
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

    [HttpGet("{id}")]
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
        var result = await _service.StartTripAsync(request);
        return Ok(result);
    }

    [HttpPut("{id}/end")]
    public async Task<IActionResult> EndTrip(int id, EndTripRequestDto request)
    {
        var result = await _service.EndTripAsync(id, request);
        return Ok(result);
    }
}