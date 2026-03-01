using Models.DTO.User;

namespace Service.Services.Interfaces
{
    public interface IUserService
    {
        Task<UserProfileDto> GetProfileAsync(int userId);
    }
}
