using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.DTO.DisposalProposals;
using Service.Services.DisposalProposals.Interfaces;
using System.Security.Claims;

namespace API.Controllers;

[ApiController]
[Authorize]
[Route("api/disposal-proposals")]
public sealed class DisposalProposalsController : ControllerBase
{
    private readonly IDisposalProposalService _service;

    public DisposalProposalsController(IDisposalProposalService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = "Operator,Branch Asset Accountant,Executive Management")]
    [ProducesResponseType(typeof(DisposalProposalPagedResultDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DisposalProposalPagedResultDto>> GetList([FromQuery] DisposalProposalListQueryDto query)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var errorResult))
        {
            return errorResult!;
        }

        var result = await _service.GetListAsync(actorUserId, roles, query);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return Ok(result.Data);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Operator,Branch Asset Accountant,Executive Management")]
    [ProducesResponseType(typeof(DisposalProposalDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<DisposalProposalDto>> GetById([FromRoute] int id)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var errorResult))
        {
            return errorResult!;
        }

        var result = await _service.GetByIdAsync(actorUserId, roles, id);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return Ok(result.Data);
    }

    [HttpPost]
    [Authorize(Roles = "Operator")]
    [ProducesResponseType(typeof(DisposalProposalDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<DisposalProposalDto>> Create([FromBody] DisposalProposalCreateRequestDto request)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var errorResult))
        {
            return errorResult!;
        }

        var result = await _service.CreateAsync(actorUserId, roles, request);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        var created = result.Data!;
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}/approve")]
    [Authorize(Roles = "Executive Management")]
    [ProducesResponseType(typeof(DisposalProposalDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DisposalProposalDto>> Approve([FromRoute] int id, [FromBody] DisposalProposalApproveRequestDto? request)
    {
        if (!TryGetActor(out var actorUserId, out _, out var errorResult))
        {
            return errorResult!;
        }

        var result = await _service.ApproveAsync(id, actorUserId, request);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return Ok(result.Data);
    }

    [HttpPut("{id:int}/reject")]
    [Authorize(Roles = "Executive Management")]
    [ProducesResponseType(typeof(DisposalProposalDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<DisposalProposalDto>> Reject([FromRoute] int id, [FromBody] DisposalProposalRejectRequestDto? request)
    {
        if (!TryGetActor(out var actorUserId, out _, out var errorResult))
        {
            return errorResult!;
        }

        var result = await _service.RejectAsync(id, actorUserId, request);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return Ok(result.Data);
    }

    [HttpGet("/api/vehicles/{vehicleId:int}/disposal-proposals")]
    [Authorize(Roles = "Operator,Branch Asset Accountant,Executive Management")]
    [ProducesResponseType(typeof(List<DisposalProposalDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<DisposalProposalDto>>> GetVehicleHistory([FromRoute] int vehicleId)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var errorResult))
        {
            return errorResult!;
        }

        var result = await _service.GetByVehicleIdAsync(actorUserId, roles, vehicleId);
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
