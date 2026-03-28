using Models.Models;

namespace Service.Services.Interfaces.Repository
{
    public interface IUserRepository
    {
        Task<User?> GetByIdWithBranchAsync(int userId);
        Task<List<User>> GetAllUsersWithBranchAsync(bool includeDeactivated);
        Task<List<User>> GetUsersByBranchWithBranchAsync(int branchId, bool includeDeactivated);
        Task SaveChangesAsync();
    }
}

