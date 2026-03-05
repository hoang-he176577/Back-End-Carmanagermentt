using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.DTO.User;
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

        [HttpGet("admin/accounts")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAccounts([FromQuery] bool includeDeactivated = false)
        {
            var accounts = await _userService.GetManagedAccountsAsync(includeDeactivated);
            return HandleResult(accounts, "Accounts retrieved successfully");
        }

        [HttpPost("admin/accounts")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAccount([FromBody] AdminCreateUserRequestDto request)
        {
            var account = await _userService.CreateAccountAsync(request);
            return HandleCreated(account, "Account created successfully");
        }

        [HttpPatch("admin/accounts/{id:int}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAccountStatus(int id, [FromBody] AdminUpdateUserStatusRequestDto request)
        {
            var account = await _userService.UpdateAccountStatusAsync(id, request.IsActive);
            var message = request.IsActive
                ? "Account activated successfully"
                : "Account deactivated successfully";
            return HandleResult(account, message);
        }
    }
}

