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
        Task RequestPasswordResetAsync(string email);
        Task ResetPasswordAsync(ResetPasswordRequestDto dto);
        Task RequestChangePasswordAsync(int userId, RequestChangePasswordDto dto);
        Task<bool> VerifyChangePasswordTokenAsync(string token);
        Task ConfirmChangePasswordAsync(ConfirmChangePasswordDto dto);
        Task<(int? branchId, string? branchName, bool emailVerified)?> GetUserBranchInfoAsync(int userId);
    }
}
