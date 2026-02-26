using Microsoft.EntityFrameworkCore;
using Models.Models;
using Service.Services.Interfaces.Repository;

namespace Service.Services.Implementations.Repository
{
    public class UserRepository : IUserRepository
    {
        private readonly CarManagerContext _context;

        public UserRepository(CarManagerContext context)
        {
            _context = context;
        }

        public async Task<User?> GetByIdWithBranchAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.Branch)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }
    }
}
