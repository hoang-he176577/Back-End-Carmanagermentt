using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.DTO.Accessories;
using Service.Services.Accessories.Interfaces;
using System.Security.Claims;

namespace API.Controllers.Accessories;

[ApiController]
[Authorize]
[Route("api/accessory-purchase-requests")]
public sealed class AccessoryPurchaseRequestsController : ControllerBase
{
    private readonly IAccessoryService _service;

    public AccessoryPurchaseRequestsController(IAccessoryService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = "Operator,Branch Asset Accountant,Manager,Executive Management")]
    [ProducesResponseType(typeof(List<AccessoryPurchaseRequestDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AccessoryPurchaseRequestDto>>> GetList(
        [FromQuery] int? branchId,
        [FromQuery] string? status,
        [FromQuery] int? page = null,
        [FromQuery] int? pageSize = null)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var errorResult))
        {
            return errorResult!;
        }

        var result = await _service.GetPurchaseRequestsAsync(actorUserId, roles, branchId, status, page, pageSize);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return Ok(result.Data);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Operator,Branch Asset Accountant,Manager,Executive Management")]
    [ProducesResponseType(typeof(AccessoryPurchaseRequestDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AccessoryPurchaseRequestDto>> GetById([FromRoute] int id)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var errorResult))
        {
            return errorResult!;
        }

        var result = await _service.GetPurchaseRequestByIdAsync(actorUserId, roles, id);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return Ok(result.Data);
    }

    [HttpPost]
    [Authorize(Roles = "Operator,Branch Asset Accountant,Manager,Executive Management")]
    [ProducesResponseType(typeof(AccessoryPurchaseRequestDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<AccessoryPurchaseRequestDto>> Create([FromBody] AccessoryPurchaseRequestCreateRequestDto request)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var errorResult))
        {
            return errorResult!;
        }

        var result = await _service.CreatePurchaseRequestAsync(actorUserId, roles, request);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return CreatedAtAction(nameof(GetById), new { id = result.Data!.Id }, result.Data);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Operator,Branch Asset Accountant,Manager,Executive Management")]
    [ProducesResponseType(typeof(AccessoryPurchaseRequestDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AccessoryPurchaseRequestDto>> Update([FromRoute] int id, [FromBody] AccessoryPurchaseRequestUpdateRequestDto request)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var errorResult))
        {
            return errorResult!;
        }

        var result = await _service.UpdatePurchaseRequestAsync(actorUserId, roles, id, request);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return Ok(result.Data);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Operator,Branch Asset Accountant,Manager,Executive Management")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> Delete([FromRoute] int id)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var errorResult))
        {
            return errorResult!;
        }

        var result = await _service.DeletePurchaseRequestAsync(actorUserId, roles, id);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return NoContent();
    }

    [HttpPost("{id:int}/approve")]
    [Authorize(Roles = "Branch Asset Accountant,Manager,Executive Management")]
    [ProducesResponseType(typeof(AccessoryPurchaseRequestDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AccessoryPurchaseRequestDto>> Approve([FromRoute] int id, [FromBody] AccessoryPurchaseRequestApproveRequestDto request)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var errorResult))
        {
            return errorResult!;
        }

        var result = await _service.ApprovePurchaseRequestAsync(actorUserId, roles, id, request);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return Ok(result.Data);
    }

    [HttpPost("{id:int}/reject")]
    [Authorize(Roles = "Branch Asset Accountant,Manager,Executive Management")]
    [ProducesResponseType(typeof(AccessoryPurchaseRequestDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AccessoryPurchaseRequestDto>> Reject([FromRoute] int id, [FromBody] AccessoryPurchaseRequestRejectRequestDto request)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var errorResult))
        {
            return errorResult!;
        }

        var result = await _service.RejectPurchaseRequestAsync(actorUserId, roles, id, request);
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
