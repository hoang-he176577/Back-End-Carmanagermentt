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

        // ───────────────── Admin Account Management ─────────────────

        [HttpGet("admin/accounts")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAdminAccounts([FromQuery] bool includeDeactivated = false)
        {
            var accounts = await _userService.GetAdminAccountsAsync(includeDeactivated);
            return HandleResult(accounts, "Accounts retrieved successfully");
        }

        [HttpPost("admin/accounts")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> CreateAdminAccount([FromBody] CreateAdminAccountDto request)
        {
            var account = await _userService.CreateAdminAccountAsync(request);
            var message = string.IsNullOrWhiteSpace(account.Warning)
                ? "Account created successfully"
                : "Account created, but verification email was not sent.";
            return HandleCreated(account, message);
        }

        [HttpPatch("admin/accounts/{id:int}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAccountStatus([FromRoute] int id, [FromBody] UpdateAccountStatusDto request)
        {
            await _userService.UpdateAccountStatusAsync(id, request.IsActive);
            return HandleSuccess(request.IsActive ? "Account activated" : "Account deactivated");
        }
    }
}

