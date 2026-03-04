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

        public async Task<List<User>> GetUsersWithDetailsAsync(bool includeDeactivated)
        {
            var query = _context.Users
                .AsNoTracking()
                .Include(u => u.Branch)
                .Include(u => u.Roles)
                .AsQueryable();

            if (!includeDeactivated)
            {
                query = query.Where(u => u.DeletedAt == null);
            }

            return await query
                .OrderByDescending(u => u.CreatedAt)
                .ThenByDescending(u => u.Id)
                .ToListAsync();
        }

        public async Task<User?> GetByIdWithDetailsAsync(int userId)
        {
            return await _context.Users
                .Include(u => u.Branch)
                .Include(u => u.Roles)
                .FirstOrDefaultAsync(u => u.Id == userId);
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public Task<bool> BranchExistsAsync(int branchId)
        {
            return _context.Branches.AnyAsync(b => b.Id == branchId);
        }

        public async Task<Role?> GetRoleByNameAsync(string roleName)
        {
            return await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
        }

        public async Task AddAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
