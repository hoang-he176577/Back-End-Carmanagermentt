
using Models.Models;

namespace Service.Services.Interfaces.Repository
{
    public interface IAuthRepository
    {
        Task<User?> GetByUsernameAsync(string username);
        Task<List<string>> GetUserRolesAsync(int userId);
        Task SaveRefreshTokenAsync(int userId, string refreshToken, DateTime expiry);
    }
}
