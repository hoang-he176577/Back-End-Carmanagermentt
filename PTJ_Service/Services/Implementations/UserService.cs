using Data.Repositories.Auth.Interfaces;
using Models.DTO.User;
using Service.Exceptions;
using Service.Services.Interfaces;
using Service.Services.Interfaces.Repository;

namespace Service.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly IAuthRepository _authRepo;

        public UserService(IUserRepository userRepo, IAuthRepository authRepo)
        {
            _userRepo = userRepo;
            _authRepo = authRepo;
        }

        public async Task<UserProfileDto> GetProfileAsync(int userId)
        {
            var user = await _userRepo.GetByIdWithBranchAsync(userId);
            if (user == null)
                throw BusinessErrors.NotFound("User not found.");

            var roles = await _authRepo.GetUserRolesAsync(userId);

            return new UserProfileDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                EmailVerified = user.EmailVerified,
                BranchId = user.BranchId,
                BranchName = user.Branch?.Name,
                Roles = roles,
                CreatedAt = user.CreatedAt,
                LastLogin = user.LastLogin
            };
        }
    }
}
