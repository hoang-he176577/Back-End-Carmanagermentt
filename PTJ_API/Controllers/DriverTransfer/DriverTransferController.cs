using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.DTO.DriverTransfer;
using Service.Services.DriverTransfer.Interfaces;
using System.Security.Claims;

namespace API.Controllers.DriverTransfer;

[ApiController]
[Authorize(Roles = "Operator,Executive Management")]
[Route("api/driver-transfers")]
public sealed class DriverTransferController : ControllerBase
{
    private readonly IDriverTransferService _service;

    public DriverTransferController(IDriverTransferService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = "Operator")]
    public async Task<IActionResult> GetAll([FromQuery] string? status)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var error)) return error!;
        var result = await _service.GetAllAsync(actorUserId, roles, status);
        return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Message });
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Operator")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var error)) return error!;
        var result = await _service.GetByIdAsync(actorUserId, roles, id);
        return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Message });
    }

    [HttpPost]
    [Authorize(Roles = "Operator")]
    public async Task<IActionResult> Create([FromBody] CreateDriverTransferRequestDto request)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var error)) return error!;
        var result = await _service.CreateAsync(actorUserId, roles, request);
        return result.Success ? StatusCode(result.StatusCode, result.Data) : StatusCode(result.StatusCode, new { message = result.Message });
    }

    [HttpPost("{id:int}/confirm")]
    [Authorize(Roles = "Operator")]
    public async Task<IActionResult> Confirm([FromRoute] int id, [FromBody] ConfirmDriverTransferDto request)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var error)) return error!;
        var result = await _service.ConfirmAsync(actorUserId, roles, id, request);
        return result.Success ? Ok(result.Data) : StatusCode(result.StatusCode, new { message = result.Message });
    }

    [HttpPut("{id:int}/cancel")]
    public async Task<IActionResult> Cancel([FromRoute] int id)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var error)) return error!;
        var result = await _service.CancelAsync(actorUserId, roles, id);
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
