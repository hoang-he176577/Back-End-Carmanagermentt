using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using Models.DTO.Auth;
using Models.Models;
using Service.Exceptions;
using Service.Services.Auth.Interfaces;
using Data.Repositories.Auth.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Service.Services.Auth.Implementations
{
    public class TokenService : ITokenService
    {
        private static readonly Dictionary<string, DateTime> RevokedRefreshJtis = new();
        private static readonly object LockObj = new();

        private readonly IConfiguration _config;
        private readonly IAuthRepository _authRepository;

        public TokenService(IConfiguration config, IAuthRepository authRepository)
        {
            _config = config;
            _authRepository = authRepository;
        }

        public Task<LoginResponseDto> IssueAsync(User user, List<string> roles, string? deviceInfo, string? ip)
        {
            CleanupRevokedEntries();

            var accessExpireMinutes = int.Parse(_config["Jwt:ExpireMinutes"] ?? "60");
            var refreshExpireDays = int.Parse(_config["Jwt:RefreshExpireDays"] ?? "7");

            var accessClaims = BuildUserClaims(user, roles);
            var refreshClaims = new List<Claim>(accessClaims)
            {
                new Claim("token_type", "refresh"),
                new Claim("device", deviceInfo ?? string.Empty),
                new Claim("ip", ip ?? string.Empty)
            };

            var accessToken = GenerateJwt(accessClaims, DateTime.UtcNow.AddMinutes(accessExpireMinutes), includeJti: true);
            var refreshToken = GenerateJwt(refreshClaims, DateTime.UtcNow.AddDays(refreshExpireDays), includeJti: true);

            return Task.FromResult(new LoginResponseDto
            {
                AccessToken = accessToken.token,
                RefreshToken = refreshToken.token,
                ExpiresIn = (int)(accessToken.expiresAtUtc - DateTime.UtcNow).TotalSeconds,
                User = new UserInfoDto
                {
                    Id = user.Id.ToString(),
                    Email = user.Email ?? string.Empty,
                    FullName = user.Name ?? string.Empty,
                    Roles = roles
                }
            });
        }

        public async Task<LoginResponseDto> RefreshAsync(string refreshToken, string? deviceInfo, string? ip)
        {
            CleanupRevokedEntries();

            var principal = ValidateJwt(refreshToken, validateLifetime: true);
            var tokenType = principal.FindFirst("token_type")?.Value;
            if (!string.Equals(tokenType, "refresh", StringComparison.Ordinal))
            {
                throw BusinessErrors.Unauthorized("Invalid refresh token.");
            }

            var oldJti = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            if (string.IsNullOrWhiteSpace(oldJti) || IsRefreshTokenRevoked(oldJti))
            {
                throw BusinessErrors.Unauthorized("Refresh token has been revoked.");
            }

            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? principal.FindFirst(JwtRegisteredClaimNames.Sub)?.Value;
            if (!int.TryParse(userIdClaim, out var userId))
            {
                throw BusinessErrors.Unauthorized("Invalid refresh token subject.");
            }

            var user = await _authRepository.GetByIdAsync(userId);
            if (user == null)
            {
                throw BusinessErrors.Unauthorized("User not found.");
            }

            var roles = await _authRepository.GetUserRolesAsync(userId);
            RevokeJti(oldJti, DateTime.UtcNow.AddDays(7));

            return await IssueAsync(user, roles, deviceInfo, ip);
        }

        public Task RevokeAsync(string refreshToken)
        {
            CleanupRevokedEntries();

            var principal = ValidateJwt(refreshToken, validateLifetime: false);
            var tokenType = principal.FindFirst("token_type")?.Value;
            if (!string.Equals(tokenType, "refresh", StringComparison.Ordinal))
            {
                return Task.CompletedTask;
            }

            var jti = principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value;
            var exp = principal.FindFirst(JwtRegisteredClaimNames.Exp)?.Value;
            if (string.IsNullOrWhiteSpace(jti))
            {
                return Task.CompletedTask;
            }

            var revokeUntil = DateTime.UtcNow.AddDays(7);
            if (long.TryParse(exp, out var unixExp))
            {
                revokeUntil = DateTimeOffset.FromUnixTimeSeconds(unixExp).UtcDateTime;
            }

            RevokeJti(jti, revokeUntil);
            return Task.CompletedTask;
        }

        private List<Claim> BuildUserClaims(User user, IEnumerable<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
                new Claim(ClaimTypes.Name, user.Email ?? string.Empty),
                new Claim("full_name", user.Name ?? string.Empty),
                new Claim("verified", (user.EmailVerified ?? false).ToString().ToLowerInvariant())
            };

            if (user.BranchId.HasValue)
            {
                claims.Add(new Claim("branchId", user.BranchId.Value.ToString()));
            }

            claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));
            return claims;
        }

        private (string token, DateTime expiresAtUtc) GenerateJwt(IEnumerable<Claim> claims, DateTime expiresAtUtc, bool includeJti)
        {
            var secret = _config["Jwt:Secret"];
            if (string.IsNullOrWhiteSpace(secret))
            {
                throw BusinessErrors.BadRequest("Jwt:Secret missing in configuration.");
            }
            if (Encoding.UTF8.GetByteCount(secret) < 32)
            {
                throw BusinessErrors.BadRequest("Jwt:Secret must be at least 32 bytes for HS256.");
            }

            var issuer = _config["Jwt:Issuer"] ?? "CarManagement.API";
            var audience = _config["Jwt:Audience"] ?? "CarManagement.Client";

            var tokenClaims = claims.ToList();
            if (includeJti)
            {
                tokenClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N")));
            }

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: issuer,
                audience: audience,
                claims: tokenClaims,
                expires: expiresAtUtc,
                signingCredentials: credentials);

            return (new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
        }

        private ClaimsPrincipal ValidateJwt(string token, bool validateLifetime)
        {
            var secret = _config["Jwt:Secret"];
            if (string.IsNullOrWhiteSpace(secret))
            {
                throw BusinessErrors.BadRequest("Jwt:Secret missing in configuration.");
            }
            if (Encoding.UTF8.GetByteCount(secret) < 32)
            {
                throw BusinessErrors.BadRequest("Jwt:Secret must be at least 32 bytes for HS256.");
            }

            var issuer = _config["Jwt:Issuer"] ?? "CarManagement.API";
            var audience = _config["Jwt:Audience"] ?? "CarManagement.Client";

            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(secret);

            var parameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidIssuer = issuer,
                ValidAudience = audience,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(key),
                ValidateLifetime = validateLifetime,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                return tokenHandler.ValidateToken(token, parameters, out _);
            }
            catch
            {
                throw BusinessErrors.Unauthorized("Invalid token.");
            }
        }

        private static bool IsRefreshTokenRevoked(string jti)
        {
            lock (LockObj)
            {
                if (!RevokedRefreshJtis.TryGetValue(jti, out var expiry))
                {
                    return false;
                }

                if (expiry <= DateTime.UtcNow)
                {
                    RevokedRefreshJtis.Remove(jti);
                    return false;
                }

                return true;
            }
        }

        private static void RevokeJti(string jti, DateTime expiresAtUtc)
        {
            lock (LockObj)
            {
                RevokedRefreshJtis[jti] = expiresAtUtc;
            }
        }

        private static void CleanupRevokedEntries()
        {
            lock (LockObj)
            {
                var now = DateTime.UtcNow;
                var expired = RevokedRefreshJtis
                    .Where(x => x.Value <= now)
                    .Select(x => x.Key)
                    .ToList();

                foreach (var key in expired)
                {
                    RevokedRefreshJtis.Remove(key);
                }
            }
        }
    }
}
