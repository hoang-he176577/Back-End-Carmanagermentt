using Models.DTO.Auth;

namespace Service.Services.Auth.Interfaces
{
    public interface IAuthService
    {
        Task<LoginResponseDto> RegisterAsync(RegisterRequestDto dto, string? ip);
        Task<LoginResponseDto> LoginAsync(LoginRequestDto dto, string? ip);
        Task<LoginResponseDto> RefreshAsync(string refreshToken, string? deviceInfo, string? ip);
        Task LogoutAsync(string refreshToken);
        Task VerifyEmailAsync(string token);
        Task ResendVerificationAsync(string email);
        Task<(int? branchId, string? branchName, bool emailVerified)?> GetUserBranchInfoAsync(int userId);
    }
}
