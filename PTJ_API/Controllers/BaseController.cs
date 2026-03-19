using API.Middlewares;
using Microsoft.AspNetCore.Mvc;
using Models.Common;
using Models.Models;
using Service.Exceptions;
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
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId))
                throw BusinessErrors.Unauthorized("Invalid user ID in token.");

            return userId;
        }
        // ===== BRANCH ID FROM JWT (THÊM MỚI ĐỂ HẾT LỖI CS0103) =====
        protected int GetBranchId()
        {
           
            var branchIdClaim = User.FindFirst("branchId")?.Value;

            if (!int.TryParse(branchIdClaim, out var branchId))
                
                throw BusinessErrors.Unauthorized("Chi nhánh không hợp lệ hoặc không tồn tại trong token.");

            return branchId;
        }

        // ===== ROLES LIST FROM JWT =====
        protected List<string>? GetUserRoles()
        {
            // assume roles claim stored as comma-separated string
            var rolesClaim = User.FindFirst("roles")?.Value;
            if (string.IsNullOrWhiteSpace(rolesClaim))
                return null;

            return rolesClaim.Split(',').Select(r => r.Trim()).ToList();
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
