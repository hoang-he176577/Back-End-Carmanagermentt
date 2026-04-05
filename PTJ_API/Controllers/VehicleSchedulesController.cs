using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.DTO.Schedules;
using Service.Services.VehicleSchedules.Interfaces;

namespace API.Controllers;

[ApiController]
[Authorize]
[Route("api/schedules")]
public sealed class VehicleSchedulesController : BaseController
{
    private readonly IVehicleScheduleService _service;

    public VehicleSchedulesController(IVehicleScheduleService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetSchedules(
        [FromQuery] DateTime? from,
        [FromQuery] DateTime? to,
        [FromQuery] int? vehicleId,
        [FromQuery] int? driverId,
        [FromQuery] int? modelId,
        [FromQuery] int? seats,
        [FromQuery] string? status,
        [FromQuery] int? branchId)
    {
        var isExecutive = IsExecutive();
        var effectiveBranchId = isExecutive ? (branchId ?? 0) : await ResolveBranchIdAsync();
        var result = await _service.GetSchedulesAsync(effectiveBranchId, from, to, vehicleId, driverId, modelId, seats, status);
        return Ok(result);
    }

    [HttpGet("availability")]
    public async Task<IActionResult> GetAvailability(
        [FromQuery] DateTime start,
        [FromQuery] DateTime end,
        [FromQuery] int? modelId,
        [FromQuery] int? branchId)
    {
        var isExecutive = IsExecutive();
        var effectiveBranchId = isExecutive ? (branchId ?? 0) : await ResolveBranchIdAsync();
        if (effectiveBranchId == 0)
        {
            return BadRequest(new { message = "branchId is required for executive availability queries." });
        }
        var result = await _service.GetAvailableVehiclesAsync(effectiveBranchId, modelId, start, end);
        return Ok(result);
    }

    [HttpGet("availability/drivers")]
    public async Task<IActionResult> GetDriverAvailability(
        [FromQuery] DateTime start,
        [FromQuery] DateTime end,
        [FromQuery] int? branchId)
    {
        var isExecutive = IsExecutive();
        var effectiveBranchId = isExecutive ? (branchId ?? 0) : await ResolveBranchIdAsync();
        if (effectiveBranchId == 0)
        {
            return BadRequest(new { message = "branchId is required for executive availability queries." });
        }
        var result = await _service.GetAvailableDriversAsync(effectiveBranchId, start, end);
        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> CreateSchedule([FromBody] VehicleScheduleCreateRequestDto request)
    {
        if (IsExecutive() || !CanCreate())
        {
            return Forbid();
        }
        var isExecutive = IsExecutive();
        var branchId = isExecutive ? (request?.BranchId ?? 0) : await ResolveBranchIdAsync();
        if (branchId == 0)
        {
            return BadRequest(new { message = "branchId is required for executive schedule creation." });
        }
        var actorUserId = GetUserId();
        var result = await _service.CreateScheduleAsync(branchId, actorUserId, request);
        return StatusCode(201, result);
    }

    [HttpPut("{scheduleId:int}")]
    public async Task<IActionResult> UpdateSchedule([FromRoute] int scheduleId, [FromBody] VehicleScheduleUpdateRequestDto request)
    {
        if (IsExecutive() || !CanReschedule())
        {
            return Forbid();
        }
        var branchId = IsExecutive() ? 0 : await ResolveBranchIdAsync();
        var actorUserId = GetUserId();
        var result = await _service.UpdateScheduleAsync(scheduleId, branchId, actorUserId, request);
        return Ok(result);
    }

    [HttpPost("{scheduleId:int}/extend")]
    public async Task<IActionResult> ExtendSchedule([FromRoute] int scheduleId, [FromBody] VehicleScheduleExtendRequestDto request)
    {
        if (IsExecutive() || !CanExtend())
        {
            return Forbid();
        }
        var branchId = IsExecutive() ? 0 : await ResolveBranchIdAsync();
        var actorUserId = GetUserId();
        var result = await _service.ExtendScheduleAsync(scheduleId, branchId, actorUserId, request);
        if (!result.Extended)
        {
            return Conflict(result);
        }

        return Ok(result);
    }

    [HttpPost("{scheduleId:int}/start")]
    public async Task<IActionResult> StartSchedule([FromRoute] int scheduleId, [FromBody] VehicleScheduleActionRequestDto request)
    {
        if (IsExecutive() || !CanOperate())
        {
            return Forbid();
        }
        var branchId = IsExecutive() ? 0 : await ResolveBranchIdAsync();
        var actorUserId = GetUserId();
        var result = await _service.StartScheduleAsync(scheduleId, branchId, actorUserId, request);
        return Ok(result);
    }

    [HttpPost("{scheduleId:int}/end")]
    public async Task<IActionResult> EndSchedule([FromRoute] int scheduleId, [FromBody] VehicleScheduleActionRequestDto request)
    {
        if (IsExecutive() || !CanOperate())
        {
            return Forbid();
        }
        var branchId = IsExecutive() ? 0 : await ResolveBranchIdAsync();
        var actorUserId = GetUserId();
        var result = await _service.EndScheduleAsync(scheduleId, branchId, actorUserId, request);
        return Ok(result);
    }

    [HttpGet("{scheduleId:int}/audits")]
    public async Task<IActionResult> GetAuditsBySchedule([FromRoute] int scheduleId)
    {
        var branchId = IsExecutive() ? 0 : await ResolveBranchIdAsync();
        var result = await _service.GetAuditsByScheduleAsync(scheduleId, branchId);
        return Ok(result);
    }

    [HttpGet("audits")]
    public async Task<IActionResult> GetRecentAudits([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        var branchId = IsExecutive() ? 0 : await ResolveBranchIdAsync();
        var result = await _service.GetRecentAuditsAsync(branchId, from, to);
        return Ok(result);
    }

    private bool IsExecutive()
    {
        var roles = GetUserRoles() ?? new List<string>();
        return roles.Any(r => string.Equals(r, "Executive Management", StringComparison.OrdinalIgnoreCase));
    }

    private bool IsOperator()
    {
        var roles = GetUserRoles() ?? new List<string>();
        return roles.Any(r => string.Equals(r, "Operator", StringComparison.OrdinalIgnoreCase));
    }

    private bool IsAccountant()
    {
        var roles = GetUserRoles() ?? new List<string>();
        return roles.Any(r => string.Equals(r, "Branch Asset Accountant", StringComparison.OrdinalIgnoreCase));
    }

    private bool IsManager()
    {
        var roles = GetUserRoles() ?? new List<string>();
        return roles.Any(r =>
            string.Equals(r, "Manager", StringComparison.OrdinalIgnoreCase) ||
            string.Equals(r, "Executive Manager", StringComparison.OrdinalIgnoreCase));
    }

    private bool CanCreate() => IsOperator();

    private bool CanReschedule() => IsOperator();

    private bool CanExtend() => IsOperator();

    private bool CanOperate() => IsOperator();

    private async Task<int> ResolveBranchIdAsync()
    {
        try
        {
            return GetBranchId();
        }
        catch
        {
            var userId = GetUserId();
            var db = HttpContext.RequestServices.GetRequiredService<Models.Models.CarManagerContext>();
            var branchId = await db.Users
                .Where(u => u.Id == userId)
                .Select(u => u.BranchId)
                .FirstOrDefaultAsync();
            if (!branchId.HasValue || branchId.Value == 0)
            {
                throw Service.Exceptions.BusinessErrors.Unauthorized("BranchId is required.");
            }
            return branchId.Value;
        }
    }
}
