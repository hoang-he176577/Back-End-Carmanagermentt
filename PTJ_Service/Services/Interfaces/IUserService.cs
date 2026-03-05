using Models.DTO.User;

namespace Service.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileDto> GetProfileAsync(int userId);
        Task<List<AdminUserAccountDto>> GetManagedAccountsAsync(bool includeDeactivated);
        Task<AdminUserAccountDto> CreateAccountAsync(AdminCreateUserRequestDto request);
        Task<AdminUserAccountDto> UpdateAccountStatusAsync(int userId, bool isActive);
    }
}

