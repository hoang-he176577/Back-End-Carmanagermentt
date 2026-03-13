using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.DTO.Auth;
using API.Controllers;
using System.Net;
using Service.Services.Auth.Interfaces;

namespace API.Controllers.Auth
{
    [ApiController]
    [Route("api/change-password")]
    public class ChangePasswordController : BaseController
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _config;

        public ChangePasswordController(IAuthService authService, IConfiguration config)
        {
            _authService = authService;
            _config = config;
        }

        [Authorize]
        [HttpPost("request")]
        public async Task<IActionResult> RequestChange([FromBody] RequestChangePasswordDto request)
        {
            var userId = GetUserId();
            await _authService.RequestChangePasswordAsync(userId, request);
            return HandleSuccess("Confirmation email for password change has been sent.");
        }

        [AllowAnonymous]
        [HttpGet("verify")]
        public async Task<IActionResult> VerifyToken([FromQuery] string token)
        {
            var allowed = await _authService.VerifyChangePasswordTokenAsync(token);
            if (!allowed)
            {
                throw Service.Exceptions.BusinessErrors.BadRequest("Token is invalid or has expired.");
            }

            var frontendBase = (_config["Frontend:BaseUrl"] ?? "http://localhost:5173").TrimEnd('/');
            var decodedToken = WebUtility.UrlDecode(token).Trim();
            var redirectUrl = $"{frontendBase}/set-new-password?token={WebUtility.UrlEncode(decodedToken)}";
            return Redirect(redirectUrl);
        }

        [AllowAnonymous]
        [HttpPost("confirm")]
        public async Task<IActionResult> Confirm([FromBody] ConfirmChangePasswordDto request)
        {
            await _authService.ConfirmChangePasswordAsync(request);
            return HandleSuccess("Password changed successfully.");
        }
    }
}
