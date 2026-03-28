
using Models.DTO.Auth;
using Models.Models;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Configuration;
using Service.Exceptions;
using Service.Services.Auth.Interfaces;
using Data.Repositories.Auth.Interfaces;
using System.Net;
using System.Security.Cryptography;

namespace Service.Services.Auth.Implementations
{
    public class AuthService : IAuthService
    {
        private const string ResetPasswordTokenPrefix = "pwd-reset.";
        private const string ChangePasswordTokenPrefix = "pwd-change.";

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

            var requestedRole = dto.Role?.Trim();
            if (string.IsNullOrWhiteSpace(requestedRole))
            {
                throw BusinessErrors.BadRequest("Role is required.");
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

            var added = await _repo.AddRoleToUserAsync(created.Id, requestedRole);
            if (!added)
            {
                throw BusinessErrors.BadRequest($"Invalid role '{requestedRole}'.");
            }
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

            if (user.DeletedAt != null)
                throw BusinessErrors.Unauthorized("Your account has been deactivated.");

            if (user.EmailVerified != true)
                throw BusinessErrors.BadRequest("Email is not verified. Please verify your email before logging in.");

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

        public async Task RequestPasswordResetAsync(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                return;
            }

            var normalizedEmail = email.Trim().ToLowerInvariant();
            var user = await _repo.GetByEmailAsync(normalizedEmail);
            if (user == null || user.DeletedAt != null || string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                return;
            }

            var token = $"{ResetPasswordTokenPrefix}{WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(48))}";
            await _repo.AddEmailVerificationTokenAsync(new EmailVerificationToken
            {
                UserId = user.Id,
                Token = token,
                ExpiresAt = DateTime.UtcNow.AddMinutes(30),
                CreatedAt = DateTime.UtcNow
            });

            var frontendBaseUrl = _config["Frontend:BaseUrl"]?.TrimEnd('/');
            if (string.IsNullOrWhiteSpace(frontendBaseUrl))
            {
                throw BusinessErrors.BadRequest("Frontend:BaseUrl is missing in configuration.");
            }

            var resetLink = $"{frontendBaseUrl}/reset-password?token={WebUtility.UrlEncode(token)}";
            var safeName = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(user.Name) ? "User" : user.Name.Trim());
            var safeLink = WebUtility.HtmlEncode(resetLink);

            var html = $"""
                        <div style="background:#f4f7fb;padding:24px 0;font-family:Arial,Helvetica,sans-serif;color:#1f2937;">
                          <div style="max-width:620px;margin:0 auto;background:#ffffff;border:1px solid #e5e7eb;border-radius:14px;overflow:hidden;">
                            <div style="padding:20px 24px;background:linear-gradient(135deg,#0f6fff,#0b59cc);color:#ffffff;">
                              <h2 style="margin:0;font-size:22px;">CarManagement</h2>
                              <p style="margin:8px 0 0;font-size:14px;opacity:.95;">Reset password request</p>
                            </div>
                            <div style="padding:24px;">
                              <p style="margin:0 0 12px;">Hello <strong>{safeName}</strong>,</p>
                              <p style="margin:0 0 16px;line-height:1.6;">
                                We received a request to reset your password. This link will expire in 30 minutes.
                              </p>
                              <div style="margin:0 0 18px;">
                                <a href="{safeLink}" style="display:inline-block;background:#0f6fff;color:#ffffff;text-decoration:none;padding:10px 16px;border-radius:8px;font-weight:600;">
                                  Reset Password
                                </a>
                              </div>
                              <p style="margin:0;word-break:break-all;font-size:12px;color:#4b5563;">{safeLink}</p>
                            </div>
                          </div>
                        </div>
                        """;

            await _emailSender.SendEmailAsync(
                user.Email ?? string.Empty,
                "CarManagement - Reset your password",
                html);
        }

        public async Task ResetPasswordAsync(ResetPasswordRequestDto dto)
        {
            var decodedToken = WebUtility.UrlDecode(dto.Token).Trim();
            if (!decodedToken.StartsWith(ResetPasswordTokenPrefix, StringComparison.Ordinal))
            {
                throw BusinessErrors.BadRequest("Token is invalid or has expired.");
            }

            var tokenEntity = await _repo.GetActiveEmailVerificationTokenAsync(decodedToken);
            if (tokenEntity == null || tokenEntity.ExpiresAt <= DateTime.UtcNow || tokenEntity.User.DeletedAt != null)
            {
                throw BusinessErrors.BadRequest("Token is invalid or has expired.");
            }

            tokenEntity.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            tokenEntity.User.UpdatedAt = DateTime.UtcNow;
            tokenEntity.UsedAt = DateTime.UtcNow;
            await _repo.SaveChangesAsync();
        }

        public async Task RequestChangePasswordAsync(int userId, RequestChangePasswordDto dto)
        {
            var user = await _repo.GetByIdAsync(userId);
            if (user == null || user.DeletedAt != null)
            {
                throw BusinessErrors.Unauthorized("User does not exist.");
            }

            if (string.IsNullOrWhiteSpace(user.PasswordHash))
            {
                throw BusinessErrors.BadRequest("This account does not support password change.");
            }

            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, user.PasswordHash))
            {
                throw BusinessErrors.BadRequest("Current password is incorrect.");
            }

            var token = $"{ChangePasswordTokenPrefix}{WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(48))}";
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

            var verifyLink = $"{apiBaseUrl}/api/change-password/verify?token={WebUtility.UrlEncode(token)}";
            var safeName = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(user.Name) ? "User" : user.Name.Trim());
            var safeLink = WebUtility.HtmlEncode(verifyLink);

            var html = $"""
                        <div style="background:#f4f7fb;padding:24px 0;font-family:Arial,Helvetica,sans-serif;color:#1f2937;">
                          <div style="max-width:620px;margin:0 auto;background:#ffffff;border:1px solid #e5e7eb;border-radius:14px;overflow:hidden;">
                            <div style="padding:20px 24px;background:linear-gradient(135deg,#0f6fff,#0b59cc);color:#ffffff;">
                              <h2 style="margin:0;font-size:22px;">CarManagement</h2>
                              <p style="margin:8px 0 0;font-size:14px;opacity:.95;">Confirm password change</p>
                            </div>
                            <div style="padding:24px;">
                              <p style="margin:0 0 12px;">Hello <strong>{safeName}</strong>,</p>
                              <p style="margin:0 0 16px;line-height:1.6;">
                                We received a request to change your password. Confirm this request within 30 minutes.
                              </p>
                              <div style="margin:0 0 18px;">
                                <a href="{safeLink}" style="display:inline-block;background:#0f6fff;color:#ffffff;text-decoration:none;padding:10px 16px;border-radius:8px;font-weight:600;">
                                  Confirm Password Change
                                </a>
                              </div>
                              <p style="margin:0;word-break:break-all;font-size:12px;color:#4b5563;">{safeLink}</p>
                            </div>
                          </div>
                        </div>
                        """;

            await _emailSender.SendEmailAsync(
                user.Email ?? string.Empty,
                "CarManagement - Confirm password change",
                html);
        }

        public async Task<bool> VerifyChangePasswordTokenAsync(string token)
        {
            var decodedToken = WebUtility.UrlDecode(token).Trim();
            if (!decodedToken.StartsWith(ChangePasswordTokenPrefix, StringComparison.Ordinal))
            {
                return false;
            }

            var tokenEntity = await _repo.GetActiveEmailVerificationTokenAsync(decodedToken);
            return tokenEntity != null && tokenEntity.ExpiresAt > DateTime.UtcNow;
        }

        public async Task ConfirmChangePasswordAsync(ConfirmChangePasswordDto dto)
        {
            var decodedToken = WebUtility.UrlDecode(dto.Token).Trim();
            if (!decodedToken.StartsWith(ChangePasswordTokenPrefix, StringComparison.Ordinal))
            {
                throw BusinessErrors.BadRequest("Token is invalid or has expired.");
            }

            var tokenEntity = await _repo.GetActiveEmailVerificationTokenAsync(decodedToken);
            if (tokenEntity == null || tokenEntity.ExpiresAt <= DateTime.UtcNow || tokenEntity.User.DeletedAt != null)
            {
                throw BusinessErrors.BadRequest("Token is invalid or has expired.");
            }

            if (!string.Equals(dto.NewPassword, dto.ConfirmNewPassword, StringComparison.Ordinal))
            {
                throw BusinessErrors.BadRequest("Confirm password does not match.");
            }

            tokenEntity.User.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            tokenEntity.User.UpdatedAt = DateTime.UtcNow;
            tokenEntity.UsedAt = DateTime.UtcNow;
            await _repo.SaveChangesAsync();
        }

        private async Task CreateAndSendVerificationTokenAsync(User user)
        {
            await _repo.InvalidateActiveEmailVerificationTokensAsync(user.Id);

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
            var roles = await _repo.GetUserRolesAsync(user.Id);
            var roleText = roles.Count > 0 ? string.Join(", ", roles) : "Not assigned";
            var createdAtText = (user.CreatedAt ?? DateTime.UtcNow).ToString("yyyy-MM-dd HH:mm:ss 'UTC'");
            var safeName = WebUtility.HtmlEncode(string.IsNullOrWhiteSpace(user.Name) ? "User" : user.Name.Trim());
            var safeEmail = WebUtility.HtmlEncode(user.Email ?? string.Empty);
            var safeRoles = WebUtility.HtmlEncode(roleText);
            var safeCreatedAt = WebUtility.HtmlEncode(createdAtText);
            var safeLink = WebUtility.HtmlEncode(verifyLink);

            var html = $"""
                        <div style="background:#f4f7fb;padding:24px 0;font-family:Arial,Helvetica,sans-serif;color:#1f2937;">
                          <div style="max-width:620px;margin:0 auto;background:#ffffff;border:1px solid #e5e7eb;border-radius:14px;overflow:hidden;">
                            <div style="padding:20px 24px;background:linear-gradient(135deg,#0f6fff,#0b59cc);color:#ffffff;">
                              <h2 style="margin:0;font-size:22px;">CarManagement</h2>
                              <p style="margin:8px 0 0;font-size:14px;opacity:.95;">Account verification required</p>
                            </div>
                            <div style="padding:24px;">
                              <p style="margin:0 0 12px;">Hello <strong>{safeName}</strong>,</p>
                              <p style="margin:0 0 16px;line-height:1.6;">
                                Your account has been created successfully. Please verify your email to activate access.
                              </p>

                              <table style="width:100%;border-collapse:collapse;margin:0 0 18px;">
                                <tr>
                                  <td style="padding:10px;border:1px solid #e5e7eb;background:#f9fafb;width:180px;"><strong>Full name</strong></td>
                                  <td style="padding:10px;border:1px solid #e5e7eb;">{safeName}</td>
                                </tr>
                                <tr>
                                  <td style="padding:10px;border:1px solid #e5e7eb;background:#f9fafb;"><strong>Email</strong></td>
                                  <td style="padding:10px;border:1px solid #e5e7eb;">{safeEmail}</td>
                                </tr>
                                <tr>
                                  <td style="padding:10px;border:1px solid #e5e7eb;background:#f9fafb;"><strong>Role</strong></td>
                                  <td style="padding:10px;border:1px solid #e5e7eb;">{safeRoles}</td>
                                </tr>
                                <tr>
                                  <td style="padding:10px;border:1px solid #e5e7eb;background:#f9fafb;"><strong>Created at</strong></td>
                                  <td style="padding:10px;border:1px solid #e5e7eb;">{safeCreatedAt}</td>
                                </tr>
                              </table>

                              <p style="margin:0 0 14px;">Verification link (expires in 30 minutes):</p>
                              <div style="margin:0 0 18px;">
                                <a href="{safeLink}" style="display:inline-block;background:#0f6fff;color:#ffffff;text-decoration:none;padding:10px 16px;border-radius:8px;font-weight:600;">
                                  Verify Email
                                </a>
                              </div>
                              <p style="margin:0;word-break:break-all;font-size:12px;color:#4b5563;">{safeLink}</p>
                            </div>
                            <div style="padding:14px 24px;background:#f9fafb;border-top:1px solid #e5e7eb;font-size:12px;color:#6b7280;">
                              If you did not expect this account, contact your administrator.
                            </div>
                          </div>
                        </div>
                        """;

            await _emailSender.SendEmailAsync(
                user.Email ?? string.Empty,
                "CarManagement - Verify your account",
                html);
        }

        public async Task<(int? branchId, string? branchName, bool emailVerified)?> GetUserBranchInfoAsync(int userId)
        {
            var user = await _repo.GetByIdAsync(userId);
            if (user == null) return null;

            string? branchName = null;
            if (user.BranchId.HasValue)
            {
                branchName = (await _repo.GetBranchNameAsync(user.BranchId.Value));
            }

            return (user.BranchId, branchName, user.EmailVerified == true);
        }
    }
}
