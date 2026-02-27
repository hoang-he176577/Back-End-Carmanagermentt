using Microsoft.EntityFrameworkCore;
using Models.Models;
using Data.Repositories.Auth.Interfaces;

namespace Data.Repositories.Auth.Implementations;

public class AuthRepository : IAuthRepository
{
    private readonly CarManagerContext _context;

    public AuthRepository(CarManagerContext context)
    {
        _context = context;
    }

    public async Task<bool> EmailExistsAsync(string email)
    {
        return await _context.Users.AnyAsync(x => x.Email == email);
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .FirstOrDefaultAsync(x => x.Email == username);
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(x => x.Email == email);
    }

    public async Task<bool> BranchExistsAsync(int branchId)
    {
        return await _context.Branches.AnyAsync(b => b.Id == branchId);
    }

    public async Task<User?> GetByIdAsync(int userId)
    {
        return await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
    }

    public async Task<User> CreateUserAsync(User user)
    {
        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool> AddRoleToUserAsync(int userId, string roleName)
    {
        var user = await _context.Users
            .Include(u => u.Roles)
            .FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
        {
            return false;
        }

        var role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName);
        if (role == null)
        {
            return false;
        }

        if (user.Roles.All(r => r.Id != role.Id))
        {
            user.Roles.Add(role);
            await _context.SaveChangesAsync();
        }

        return true;
    }

    public async Task<List<string>> GetUserRolesAsync(int userId)
    {
        return await _context.Users
            .Where(u => u.Id == userId)
            .SelectMany(u => u.Roles.Select(r => r.Name))
            .Where(roleName => roleName != null)
            .Select(roleName => roleName!)
            .ToListAsync();
    }

    public async Task UpdateLastLoginAsync(int userId, DateTime loginAtUtc)
    {
        var user = await _context.Users.FirstOrDefaultAsync(x => x.Id == userId);
        if (user == null)
        {
            return;
        }

        user.LastLogin = loginAtUtc;
        await _context.SaveChangesAsync();
    }

    public async Task AddEmailVerificationTokenAsync(EmailVerificationToken token)
    {
        _context.EmailVerificationTokens.Add(token);
        await _context.SaveChangesAsync();
    }

    public async Task<EmailVerificationToken?> GetActiveEmailVerificationTokenAsync(string token)
    {
        return await _context.EmailVerificationTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token && t.UsedAt == null);
    }

    public async Task<EmailVerificationToken?> GetLatestActiveVerificationTokenByUserIdAsync(int userId)
    {
        return await _context.EmailVerificationTokens
            .Where(t => t.UserId == userId && t.UsedAt == null)
            .OrderByDescending(t => t.CreatedAt)
            .FirstOrDefaultAsync();
    }

    public async Task<string?> GetBranchNameAsync(int branchId)
    {
        return await _context.Branches.AsNoTracking()
            .Where(b => b.Id == branchId)
            .Select(b => b.Name)
            .FirstOrDefaultAsync();
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}
