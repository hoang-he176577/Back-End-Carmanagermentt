
using Microsoft.Extensions.Configuration;
using Models.DTO.Auth;
using Service.Exceptions;
using Service.Helpers;
using Service.Services.Interfaces;
using Service.Services.Interfaces.Repository;

namespace Service.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly IAuthRepository _repo;
        private readonly IConfiguration _config;

        public AuthService(IAuthRepository repo, IConfiguration config)
        {
            _repo = repo;
            _config = config;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto dto)
        {
            var user = await _repo.GetByUsernameAsync(dto.Email);
            if (user == null)
                throw BusinessErrors.BadRequest("Invalid email or password");

            bool valid = BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash);
            if (!valid)
                throw BusinessErrors.BadRequest("Invalid email or password");

            var roles = await _repo.GetUserRolesAsync(user.Id);

            string secret = _config["Jwt:Secret"]!;
            int expire = int.Parse(_config["Jwt:ExpireMinutes"]!);

            var accessToken = JwtHelper.GenerateToken(
                user.Id,
                user.Email!,
                roles,
                secret!,
                expire
            );

            var refreshToken = Guid.NewGuid().ToString();

            await _repo.SaveRefreshTokenAsync(
                user.Id,
                refreshToken,
                DateTime.UtcNow.AddDays(7)
            );

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                ExpiresIn = expire * 60,
                User = new UserInfoDto
                {
                    Id = user.Id.ToString(),
                    Email = user.Email!,
                    FullName = user.Name!,
                    Roles = roles
                }
            };
        }
    }
}
