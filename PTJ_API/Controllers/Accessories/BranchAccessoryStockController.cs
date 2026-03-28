using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.DTO.Accessories;
using Service.Services.Accessories.Interfaces;
using System.Security.Claims;

namespace API.Controllers.Accessories;

[ApiController]
[Authorize]
[Route("api/branch-accessory-stock")]
public sealed class BranchAccessoryStockController : ControllerBase
{
    private readonly IAccessoryService _service;

    public BranchAccessoryStockController(IAccessoryService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = "Operator,Branch Asset Accountant,Manager,Executive Management")]
    [ProducesResponseType(typeof(List<BranchAccessoryStockDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<BranchAccessoryStockDto>>> GetList(
        [FromQuery] int? branchId,
        [FromQuery] int? accessoryId,
        [FromQuery] bool belowMinimumOnly = false,
        [FromQuery] int? page = null,
        [FromQuery] int? pageSize = null)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var errorResult))
        {
            return errorResult!;
        }

        var result = await _service.GetBranchStocksAsync(actorUserId, roles, branchId, accessoryId, belowMinimumOnly, page, pageSize);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return Ok(result.Data);
    }

    [HttpPut]
    [Authorize(Roles = "Operator,Branch Asset Accountant,Manager,Executive Management")]
    [ProducesResponseType(typeof(BranchAccessoryStockDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<BranchAccessoryStockDto>> Upsert([FromBody] BranchAccessoryStockUpsertRequestDto request)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var errorResult))
        {
            return errorResult!;
        }

        var result = await _service.UpsertBranchStockAsync(actorUserId, roles, request);
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
