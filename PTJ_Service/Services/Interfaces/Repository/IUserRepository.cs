using Models.Models;

namespace Service.Services.Interfaces.Repository
{
    public interface IUserRepository
    {
        Task<User?> GetByIdWithBranchAsync(int userId);
        Task<List<User>> GetUsersWithDetailsAsync(bool includeDeactivated);
        Task<User?> GetByIdWithDetailsAsync(int userId);
        Task<User?> GetByEmailAsync(string email);
        Task<bool> BranchExistsAsync(int branchId);
        Task<Role?> GetRoleByNameAsync(string roleName);
        Task AddAsync(User user);
        Task SaveChangesAsync();
    }
}
