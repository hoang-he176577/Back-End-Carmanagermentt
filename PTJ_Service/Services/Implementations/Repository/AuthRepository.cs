using Microsoft.EntityFrameworkCore;
using Models.Models;
using Service.Services.Interfaces.Repository;

namespace Service.Services.Implementations.Repository
{
    public class AuthRepository : IAuthRepository
    {
        private readonly CarManagerContext _context;

        public AuthRepository(CarManagerContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByUsernameAsync(string username)
        {
            return await _context.Users
                .FirstOrDefaultAsync(x => x.Email == username);
        }

        public async Task<List<string>> GetUserRolesAsync(int userId)
        {
            var user = await _context.Users.FindAsync(userId);
            if (user == null) return new List<string>();

            var role = await _context.Roles.FindAsync(user.Roles.FirstOrDefault()!.Id);
            if (role == null) return new List<string>();

            return new List<string> { role.Name };
        }

        public async Task SaveRefreshTokenAsync(int userId, string refreshToken, DateTime expiry)
        {
            // nếu bạn có bảng refresh token thì lưu
            // demo tạm bỏ qua
            await Task.CompletedTask;
        }
    }
}
