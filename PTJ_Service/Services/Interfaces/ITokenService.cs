using Models.DTO.Auth;
using Models.Models;

namespace Service.Services.Interfaces
{
    public interface ITokenService
    {
        Task<LoginResponseDto> IssueAsync(User user, List<string> roles, string? deviceInfo, string? ip);
        Task<LoginResponseDto> RefreshAsync(string refreshToken, string? deviceInfo, string? ip);
        Task RevokeAsync(string refreshToken);
    }
}
