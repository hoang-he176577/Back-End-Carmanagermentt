using Data.Repositories.Auth.Interfaces;
using Models.DTO.User;
using Models.Models;
using Service.Exceptions;
using Service.Services.Auth.Interfaces;
using Service.Services.Interfaces;
using Service.Services.Interfaces.Repository;

namespace Service.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly IAuthRepository _authRepo;
        private readonly IAuthService _authService;

        public UserService(IUserRepository userRepo, IAuthRepository authRepo, IAuthService authService)
        {
            _userRepo = userRepo;
            _authRepo = authRepo;
            _authService = authService;
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

        // ───────────────── Admin Account Management ─────────────────

        public async Task<List<AdminAccountDto>> GetAdminAccountsAsync(bool includeDeactivated)
        {
            var users = await _userRepo.GetAllUsersWithBranchAsync(includeDeactivated);

            var result = new List<AdminAccountDto>();
            foreach (var user in users)
            {
                var roles = await _authRepo.GetUserRolesAsync(user.Id);
                result.Add(new AdminAccountDto
                {
                    Id = user.Id,
                    Name = user.Name,
                    Email = user.Email,
                    Phone = user.Phone,
                    BranchId = user.BranchId,
                    BranchName = user.Branch?.Name,
                    Roles = roles,
                    EmailVerified = user.EmailVerified,
                    IsActive = user.DeletedAt == null,
                    CreatedAt = user.CreatedAt,
                    LastLogin = user.LastLogin
                });
            }

            return result;
        }

        public async Task<AdminAccountDto> CreateAdminAccountAsync(CreateAdminAccountDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                throw BusinessErrors.BadRequest("Email is required.");

            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
                throw BusinessErrors.BadRequest("Password must be at least 6 characters.");

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            if (await _authRepo.EmailExistsAsync(normalizedEmail))
                throw BusinessErrors.Conflict("Email already exists.");

            if (request.BranchId.HasValue && !await _authRepo.BranchExistsAsync(request.BranchId.Value))
                throw BusinessErrors.BadRequest("Branch not found.");

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Name = request.Name?.Trim(),
                Email = normalizedEmail,
                PasswordHash = passwordHash,
                Phone = request.Phone?.Trim(),
                BranchId = request.BranchId,
                EmailVerified = false,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _authRepo.CreateUserAsync(user);

            if (!string.IsNullOrWhiteSpace(request.Role))
            {
                await _authRepo.AddRoleToUserAsync(created.Id, request.Role.Trim());
            }

            string? warning = null;
            try
            {
                await _authService.ResendVerificationAsync(normalizedEmail);
            }
            catch (BusinessException ex)
            {
                warning = ex.Message;
            }

            var roles = await _authRepo.GetUserRolesAsync(created.Id);
            var createdUser = await _userRepo.GetByIdWithBranchAsync(created.Id);

            return new AdminAccountDto
            {
                Id = created.Id,
                Name = createdUser?.Name,
                Email = createdUser?.Email,
                Phone = createdUser?.Phone,
                BranchId = createdUser?.BranchId,
                BranchName = createdUser?.Branch?.Name,
                Roles = roles,
                EmailVerified = createdUser?.EmailVerified,
                IsActive = createdUser?.DeletedAt == null,
                CreatedAt = createdUser?.CreatedAt,
                LastLogin = createdUser?.LastLogin,
                Warning = warning
            };
        }

        public async Task UpdateAccountStatusAsync(int id, bool isActive)
        {
            var user = await _userRepo.GetByIdWithBranchAsync(id);
            if (user == null)
                throw BusinessErrors.NotFound("User not found.");

            if (isActive)
            {
                user.DeletedAt = null;
            }
            else
            {
                user.DeletedAt = DateTime.UtcNow;
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _userRepo.SaveChangesAsync();
        }
    }
}

