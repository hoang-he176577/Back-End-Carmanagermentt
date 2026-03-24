using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.DTO.Drivers;
using Service.Services.Drivers.Interfaces;
using System.Security.Claims;

namespace API.Controllers.Drivers;

[ApiController]
[Authorize]
[Route("api/drivers")]
public sealed class DriverController : ControllerBase
{
    private readonly IDriverService _service;

    public DriverController(IDriverService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] int? branchId)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var error)) return error!;
        var result = await _service.GetAllAsync(actorUserId, roles, branchId);
        return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Message });
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var error)) return error!;
        var result = await _service.GetByIdAsync(actorUserId, roles, id);
        return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Message });
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] DriverCreateDto request)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var error)) return error!;
        var result = await _service.CreateAsync(actorUserId, roles, request);
        return result.Success ? StatusCode(result.StatusCode, result.Data) : StatusCode(result.StatusCode, new { message = result.Message });
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] DriverUpdateDto request)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var error)) return error!;
        var result = await _service.UpdateAsync(actorUserId, roles, id, request);
        return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Message });
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var error)) return error!;
        var result = await _service.DeleteAsync(actorUserId, roles, id);
        return result.Success ? Ok(new { success = true }) : StatusCode(result.StatusCode, new { message = result.Message });
    }

    [HttpGet("available")]
    public async Task<IActionResult> GetAvailable([FromQuery] int? branchId)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var error)) return error!;
        var result = await _service.GetAvailableByBranchAsync(actorUserId, roles, branchId);
        return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Message });
    }

    [HttpGet("dropdown")]
    public async Task<IActionResult> GetDropdown([FromQuery] int? branchId)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var error)) return error!;
        var result = await _service.GetDropdownAsync(actorUserId, roles, branchId);
        return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Message });
    }

    private bool TryGetActor(out int actorUserId, out IReadOnlyCollection<string> roles, out ActionResult? errorResult)
    {
        roles = User.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .Where(v => !string.IsNullOrWhiteSpace(v))
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
