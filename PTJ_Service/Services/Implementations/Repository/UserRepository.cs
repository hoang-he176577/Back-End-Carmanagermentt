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

        public async Task<List<User>> GetAllUsersWithBranchAsync(bool includeDeactivated)
        {
            var query = _context.Users
                .Include(u => u.Branch)
                .AsNoTracking();

            if (!includeDeactivated)
            {
                query = query.Where(u => u.DeletedAt == null);
            }

            return await query.OrderByDescending(u => u.CreatedAt).ToListAsync();
        }

        public Task SaveChangesAsync()
        {
            return _context.SaveChangesAsync();
        }
    }
}

