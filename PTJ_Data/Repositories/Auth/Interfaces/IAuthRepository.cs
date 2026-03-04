using Models.Models;

namespace Data.Repositories.Auth.Interfaces;

public interface IAuthRepository
{
    Task<bool> EmailExistsAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<User?> GetByIdAsync(int userId);
    Task<User?> GetByEmailAsync(string email);
    Task<bool> BranchExistsAsync(int branchId);
    Task<User> CreateUserAsync(User user);
    Task<bool> AddRoleToUserAsync(int userId, string roleName);
    Task<List<string>> GetUserRolesAsync(int userId);
    Task UpdateLastLoginAsync(int userId, DateTime loginAtUtc);
    Task AddEmailVerificationTokenAsync(EmailVerificationToken token);
    Task<EmailVerificationToken?> GetActiveEmailVerificationTokenAsync(string token);
    Task<EmailVerificationToken?> GetLatestActiveVerificationTokenByUserIdAsync(int userId);
    Task<string?> GetBranchNameAsync(int branchId);
    Task SaveChangesAsync();
}
