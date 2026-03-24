using API.Middlewares;
using Microsoft.AspNetCore.Mvc;
using Models.Common;
using Models.Models;
using Service.Exceptions;
using Microsoft.Extensions.DependencyInjection;
using System.Security.Claims;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public abstract class BaseController : ControllerBase
    {
        // ===== USER ID FROM JWT =====
        protected int GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
                ?? User.FindFirstValue("sub");

            if (!int.TryParse(userIdClaim, out var userId))
                throw BusinessErrors.Unauthorized("Invalid user ID in token.");

            return userId;
        }
        // ===== BRANCH ID FROM JWT (THÊM MỚI ĐỂ HẾT LỖI CS0103) =====
        protected int GetBranchId()
        {
           
            var branchIdClaim = User.FindFirst("branchId")?.Value
                ?? User.FindFirst("branch_id")?.Value
                ?? User.FindFirst("branch")?.Value;

            if (int.TryParse(branchIdClaim, out var branchId))
                return branchId;

            // Fallback: resolve from DB if token does not contain branchId
            var userId = GetUserId();
            var db = HttpContext?.RequestServices?.GetService<CarManagerContext>();
            if (db == null)
                throw BusinessErrors.Unauthorized("Chi nhánh không hợp lệ hoặc không tồn tại trong token.");

            var branchIdFromDb = db.Users
                .Where(u => u.Id == userId)
                .Select(u => u.BranchId)
                .FirstOrDefault();

            if (!branchIdFromDb.HasValue)
                throw BusinessErrors.Unauthorized("Chi nhánh không hợp lệ hoặc không tồn tại trong token.");

            return branchIdFromDb.Value;
        }
        // ===== STANDARD RESPONSE =====
        protected IActionResult HandleResult<T>(T result, string? message = null)
        {
            if (result == null)
                return NotFound(ApiResponse<T>.Fail(404, "Not Found"));

            return Ok(ApiResponse<T>.Ok(result, message));
        }

        protected IActionResult HandleCreated<T>(T result, string? message = null)
        {
            return StatusCode(201, ApiResponse<T>.Created(result, message));
        }

        protected IActionResult HandleSuccess(string? message = null)
        {
            return Ok(ApiResponse<string>.Ok(null!, message ?? "Success"));
        }

        protected IActionResult HandleNoContent()
        {
            return NoContent();
        }
    }
}
