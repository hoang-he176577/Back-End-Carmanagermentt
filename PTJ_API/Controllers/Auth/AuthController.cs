using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.DTO.Auth;
using Service.Services.Auth.Interfaces;
using System.Net;
using System.Security.Claims;

namespace API.Controllers.Auth
{
    public class AuthController : API.Controllers.BaseController
    {
        private readonly IAuthService _authService;
        private readonly IConfiguration _config;

        public AuthController(IAuthService authService, IConfiguration config)
        {
            _authService = authService;
            _config = config;
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _authService.RegisterAsync(request, ip);
            var message = string.IsNullOrWhiteSpace(result.Warning)
                ? "Register successful. Please check your email to verify your account."
                : "Register successful. Verification email was not sent, please check SMTP settings and resend.";
            return HandleCreated(result, message);
        }

        [HttpPost("login")]
        [AllowAnonymous]
        public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _authService.LoginAsync(request, ip);
            return HandleResult(result, "Login successful");
        }

        [HttpPost("refresh")]
        [AllowAnonymous]
        public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto request)
        {
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var result = await _authService.RefreshAsync(request.RefreshToken, request.DeviceInfo, ip);
            return HandleResult(result, "Token refreshed");
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout([FromBody] RefreshRequestDto request)
        {
            await _authService.LogoutAsync(request.RefreshToken);
            return HandleSuccess("Logout successful");
        }

        [HttpPost("verify-email")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmail([FromBody] VerifyEmailRequestDto request)
        {
            await _authService.VerifyEmailAsync(request.Token);
            return HandleSuccess("Email verification successful.");
        }

        [HttpGet("verify-email")]
        [AllowAnonymous]
        public async Task<IActionResult> VerifyEmailByLink([FromQuery] string token)
        {
            try
            {
                var decodedToken = WebUtility.UrlDecode(token);
                await _authService.VerifyEmailAsync(decodedToken);

                var frontendBase = (_config["Frontend:BaseUrl"] ?? "http://localhost:5173").TrimEnd('/');
                return Redirect($"{frontendBase}/verify-success");
            }
            catch (Exception ex)
            {
                var frontendBase = (_config["Frontend:BaseUrl"] ?? "http://localhost:5173").TrimEnd('/');
                return Redirect($"{frontendBase}/verify-failed?error={Uri.EscapeDataString(ex.Message)}");
            }
        }

        [HttpPost("resend-verification")]
        [AllowAnonymous]
        public async Task<IActionResult> ResendVerification([FromBody] ResendVerificationRequestDto request)
        {
            await _authService.ResendVerificationAsync(request.Email);
            return HandleSuccess("Verification email has been resent.");
        }

        [HttpGet("me")]
        [Authorize]
        public IActionResult Me()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? User.FindFirstValue("sub");
            var email = User.FindFirstValue(ClaimTypes.Email) ?? User.FindFirstValue("email");
            var fullName = User.FindFirstValue("full_name");
            var verified = User.FindFirstValue("verified");
            var roles = User.FindAll(ClaimTypes.Role).Select(c => c.Value);

            return Ok(new
            {
                id,
                email,
                fullName,
                verified,
                roles
            });
        }
    }
}
