
using Models.DTO.Auth;
using Models.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Service.Exceptions;
using Service.Services.Interfaces;
using Service.Services.Interfaces.Repository;
using System.Net;
using System.Security.Cryptography;

namespace Service.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _repo;
        private readonly ITokenService _tokens;
        private readonly IEmailSender _emailSender;
        private readonly IConfiguration _config;

        public AuthService(
            IAuthRepository repo,
            ITokenService tokens,
            IEmailSender emailSender,
            IConfiguration config)
        {
            _repo = repo;
            _tokens = tokens;
            _emailSender = emailSender;
            _config = config;
        }

        public async Task<LoginResponseDto> RegisterAsync(RegisterRequestDto dto, string? ip)
        {
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
            var emailExists = await _repo.EmailExistsAsync(normalizedEmail);
            if (emailExists)
            {
                throw BusinessErrors.BadRequest("Email already exists");
            }

            int? normalizedBranchId = dto.BranchId;
            if (normalizedBranchId.HasValue && normalizedBranchId.Value <= 0)
            {
                normalizedBranchId = null;
            }

            if (normalizedBranchId.HasValue)
            {
                var branchExists = await _repo.BranchExistsAsync(normalizedBranchId.Value);
                if (!branchExists)
                {
                    throw BusinessErrors.BadRequest("Branch does not exist.");
                }
            }

            var user = new User
            {
                Email = normalizedEmail,
                Name = dto.Name.Trim(),
                Phone = string.IsNullOrWhiteSpace(dto.Phone) ? null : dto.Phone.Trim(),
                BranchId = normalizedBranchId,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                EmailVerified = false,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            var created = await _repo.CreateUserAsync(user);

            const string DefaultRole = "Operator";
            await _repo.AddRoleToUserAsync(created.Id, DefaultRole);
            string? warning = null;
            try
            {
                await CreateAndSendVerificationTokenAsync(created);
            }
            catch (BusinessException ex)
            {
                warning = ex.Message;
            }

            var roles = await _repo.GetUserRolesAsync(created.Id);
            var response = await _tokens.IssueAsync(created, roles, null, ip);
            response.Warning = warning;
            return response;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto, string? ip)
        {
            var user = await _repo.GetByUsernameAsync(dto.Email.Trim().ToLowerInvariant());
            if (user == null)
                throw BusinessErrors.BadRequest("Invalid email or password");

            bool valid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!valid)
                throw BusinessErrors.BadRequest("Invalid email or password");

            var roles = await _repo.GetUserRolesAsync(user.Id);
            await _repo.UpdateLastLoginAsync(user.Id, DateTime.UtcNow);

            return await _tokens.IssueAsync(user, roles, dto.DeviceInfo, ip);
        }

        public Task<LoginResponseDto> RefreshAsync(string refreshToken, string? deviceInfo, string? ip)
            => _tokens.RefreshAsync(refreshToken, deviceInfo, ip);

        public Task LogoutAsync(string refreshToken)
            => _tokens.RevokeAsync(refreshToken);

        public async Task VerifyEmailAsync(string token)
        {
            var decodedToken = WebUtility.UrlDecode(token).Trim();
            var evToken = await _repo.GetActiveEmailVerificationTokenAsync(decodedToken);

            if (evToken == null || evToken.ExpiresAt <= DateTime.UtcNow)
            {
                throw BusinessErrors.BadRequest("Token is invalid or has expired.");
            }

            evToken.UsedAt = DateTime.UtcNow;
            evToken.User.EmailVerified = true;
            evToken.User.UpdatedAt = DateTime.UtcNow;
            await _repo.SaveChangesAsync();
        }

        public async Task ResendVerificationAsync(string email)
        {
            var normalizedEmail = email.Trim().ToLowerInvariant();
            var user = await _repo.GetByEmailAsync(normalizedEmail);
            if (user == null || user.EmailVerified == true)
            {
                return;
            }

            await CreateAndSendVerificationTokenAsync(user);
        }

        private async Task CreateAndSendVerificationTokenAsync(User user)
        {
            var token = WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(48));
            await _repo.AddEmailVerificationTokenAsync(new EmailVerificationToken
            {
                UserId = user.Id,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                CreatedAt = DateTime.UtcNow
            });

            var apiBaseUrl = _config["App:BaseUrl"]?.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(apiBaseUrl))
            {
                throw BusinessErrors.BadRequest("App:BaseUrl is missing in configuration.");
            }

            var verifyLink = $"{apiBaseUrl}/api/Auth/verify-email?token={WebUtility.UrlEncode(token)}";
            var html = $"""
                        <h3>Verify your email</h3>
                        <p>Please click the link below to verify your account:</p>
                        <p><a href="{verifyLink}">{verifyLink}</a></p>
                        <p>This link will expire in 30 minutes.</p>
                        """;

            await _emailSender.SendEmailAsync(
                user.Email ?? string.Empty,
                "CarManagement - Verify your email",
                html);
        }
    }
}
