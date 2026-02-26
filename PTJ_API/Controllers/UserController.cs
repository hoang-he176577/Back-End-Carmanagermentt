using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Exceptions;
using Service.Services.Interfaces;
using System.Security.Claims;

namespace API.Controllers
{
    public class UserController : BaseController
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<IActionResult> GetProfile()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!int.TryParse(userIdClaim, out var userId))
                throw BusinessErrors.Unauthorized("Invalid user ID in token.");

            var profile = await _userService.GetProfileAsync(userId);
            return HandleResult(profile, "Profile retrieved successfully");
        }
    }
}
