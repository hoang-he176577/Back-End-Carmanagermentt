using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.DTO.Accessories;
using Service.Services.Accessories.Interfaces;
using System.Security.Claims;

namespace API.Controllers.Accessories;

[ApiController]
[Authorize]
[Route("api/accessories")]
public sealed class AccessoriesController : ControllerBase
{
    private readonly IAccessoryService _service;

    public AccessoriesController(IAccessoryService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = "Operator,Branch Asset Accountant,Executive Management")]
    [ProducesResponseType(typeof(List<AccessoryDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AccessoryDto>>> GetList(
        [FromQuery] string? keyword,
        [FromQuery] string? type,
        [FromQuery] bool? isActive,
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var errorResult))
        {
            return errorResult!;
        }

        var result = await _service.GetAccessoriesAsync(actorUserId, roles, keyword, type, isActive, page, pageSize);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return Ok(result.Data);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Operator,Branch Asset Accountant,Executive Management")]
    [ProducesResponseType(typeof(AccessoryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<AccessoryDto>> GetById([FromRoute] int id)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var errorResult))
        {
            return errorResult!;
        }

        var result = await _service.GetAccessoryByIdAsync(actorUserId, roles, id);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return Ok(result.Data);
    }

    [HttpPost]
    [Authorize(Roles = "Operator,Executive Management")]
    [ProducesResponseType(typeof(AccessoryDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<AccessoryDto>> Create([FromBody] AccessoryCreateRequestDto request)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var errorResult))
        {
            return errorResult!;
        }

        var result = await _service.CreateAccessoryAsync(actorUserId, roles, request);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        var created = result.Data!;
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Operator,Executive Management")]
    [ProducesResponseType(typeof(AccessoryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AccessoryDto>> Update([FromRoute] int id, [FromBody] AccessoryUpdateRequestDto request)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var errorResult))
        {
            return errorResult!;
        }

        var result = await _service.UpdateAccessoryAsync(actorUserId, roles, id, request);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return Ok(result.Data);
    }

    [HttpPost("{id:int}/import")]
    [Authorize(Roles = "Operator,Executive Management")]
    [ProducesResponseType(typeof(AccessoryDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<AccessoryDto>> ImportStock([FromRoute] int id, [FromBody] AccessoryImportRequestDto request)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var errorResult))
        {
            return errorResult!;
        }

        var result = await _service.ImportAccessoryAsync(actorUserId, roles, id, request);
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
