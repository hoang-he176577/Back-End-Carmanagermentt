using Models.DTO.User;

namespace Service.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileDto> GetProfileAsync(int userId);
        Task<List<AdminAccountDto>> GetAdminAccountsAsync(bool includeDeactivated);
        Task<AdminAccountDto> CreateAdminAccountAsync(CreateAdminAccountDto request);
        Task UpdateAccountStatusAsync(int id, bool isActive);
    }
}

