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
        // ===== USER ID FROM JWT (int) =====
        protected int GetUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var userId))
                throw BusinessErrors.Unauthorized("Invalid user ID in token.");

            return userId;
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
