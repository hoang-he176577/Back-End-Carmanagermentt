using Models.DTO.User;

namespace Service.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileDto> GetProfileAsync(int userId);
        Task<List<AdminAccountDto>> GetAdminAccountsAsync(bool includeDeactivated);
        Task<AdminAccountDto> CreateAdminAccountAsync(CreateAdminAccountDto request);
        Task UpdateAccountStatusAsync(int id, bool isActive);
        Task<List<AdminAccountDto>> GetManagerAccountsAsync(int? branchId, bool includeDeactivated, bool canViewAll);
        Task<AdminAccountDto> CreateManagerAccountAsync(CreateAdminAccountDto request, int? branchId, bool canViewAll);
        Task UpdateAccountStatusForManagerAsync(int id, bool isActive, int? branchId, bool canViewAll);
    }
}

