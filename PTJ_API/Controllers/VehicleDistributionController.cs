using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.DTO.VehicleDistribution;
using Service.Services.Interfaces;

namespace API.Controllers;

[ApiController]
[Route("api/distribution")]
[Authorize]
public sealed class VehicleDistributionController : BaseController
{
    private readonly IVehicleDistributionService _service;

    public VehicleDistributionController(IVehicleDistributionService service)
    {
        _service = service;
    }

    // ───────────────── Branch Stock ─────────────────

    /// <summary>
    /// Thống kê tồn kho xe theo chi nhánh.
    /// </summary>
    [HttpGet("stock")]
    [ProducesResponseType(typeof(List<BranchStockSummaryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBranchStock()
    {
        var result = await _service.GetBranchStockAsync();
        return HandleResult(result, "Branch stock retrieved successfully.");
    }

    // ───────────────── Transfer Plans ─────────────────

    /// <summary>
    /// Danh sách kế hoạch điều chuyển. Hỗ trợ lọc theo fromBranchId, toBranchId, status.
    /// </summary>
    [HttpGet("transfers")]
    [ProducesResponseType(typeof(List<TransferPlanDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTransferPlans(
        [FromQuery] int? fromBranchId,
        [FromQuery] int? toBranchId,
        [FromQuery] string? status)
    {
        var userId = GetCurrentUserId();
        var userRole = GetCurrentUserRole();
        var result = await _service.GetTransferPlansAsync(fromBranchId, toBranchId, status, userId, userRole);
        return HandleResult(result, "Transfer plans retrieved successfully.");
    }

    /// <summary>
    /// Chi tiết một kế hoạch điều chuyển.
    /// </summary>
    [HttpGet("transfers/{id:int}")]
    [ProducesResponseType(typeof(TransferPlanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTransferPlanById([FromRoute] int id)
    {
        var result = await _service.GetTransferPlanByIdAsync(id);
        if (result == null)
            return NotFound(new { message = "Transfer plan not found." });

        return HandleResult(result, "Transfer plan retrieved successfully.");
    }

    /// <summary>
    /// Tạo kế hoạch điều chuyển mới (chỉ Branch Asset Accountant).
    /// </summary>
    [HttpPost("transfers")]
    [Authorize(Roles = "Branch Asset Accountant")]
    [ProducesResponseType(typeof(TransferPlanDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateTransferPlan([FromBody] TransferPlanCreateRequestDto request)
    {
        var userId = GetCurrentUserId();
        var result = await _service.CreateTransferPlanAsync(request, userId);

        if (!result.Success)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return HandleCreated(result.Data!, "Transfer plan created successfully.");
    }

    /// <summary>
    /// Cập nhật trạng thái kế hoạch điều chuyển (phê duyệt/từ chối/thực hiện/hủy).
    /// </summary>
    [HttpPut("transfers/{id:int}/status")]
    [Authorize(Roles = "Executive Management,Branch Asset Accountant,Operator")]
    [ProducesResponseType(typeof(TransferPlanDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateTransferPlanStatus(
        [FromRoute] int id,
        [FromBody] TransferPlanUpdateStatusDto request)
    {
        var userId = GetCurrentUserId();
        var userRole = GetCurrentUserRole();

        var result = await _service.UpdateTransferPlanStatusAsync(id, request, userId, userRole);

        if (!result.Success)
            return StatusCode(result.StatusCode, new { message = result.Message });

        return HandleResult(result.Data!, "Transfer plan status updated successfully.");
    }

    // ───────────────── Private Helpers ─────────────────

    private int GetCurrentUserId()
    {
        var claim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(claim, out var id) ? id : 0;
    }

    private string GetCurrentUserRole()
    {
        return User.FindFirstValue(ClaimTypes.Role) ?? string.Empty;
    }
}
