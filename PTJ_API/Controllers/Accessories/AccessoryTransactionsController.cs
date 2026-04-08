using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.DTO.Accessories;
using Service.Services.Accessories.Interfaces;
using System.Security.Claims;

namespace API.Controllers.Accessories;

[ApiController]
[Authorize]
[Route("api/accessory-transactions")]
public sealed class AccessoryTransactionsController : ControllerBase
{
    private readonly IAccessoryService _service;

    public AccessoryTransactionsController(IAccessoryService service)
    {
        _service = service;
    }

    [HttpGet]
    [Authorize(Roles = "Operator,Branch Asset Accountant,Manager,Executive Management")]
    [ProducesResponseType(typeof(List<AccessoryTransactionDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<AccessoryTransactionDto>>> GetList(
        [FromQuery] int? branchId,
        [FromQuery] int? accessoryId,
        [FromQuery] int? vehicleId,
        [FromQuery] string? transactionType,
        [FromQuery] string? referenceType,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate,
        [FromQuery] int? page,
        [FromQuery] int? pageSize)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var errorResult))
        {
            return errorResult!;
        }

        var result = await _service.GetTransactionsAsync(
            actorUserId,
            roles,
            branchId,
            accessoryId,
            vehicleId,
            transactionType,
            referenceType,
            fromDate,
            toDate,
            page,
            pageSize);

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
