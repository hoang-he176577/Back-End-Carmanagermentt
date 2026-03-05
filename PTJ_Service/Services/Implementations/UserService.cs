using Data.Repositories.Auth.Interfaces;
using Models.DTO.User;
using Models.Models;
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

        public async Task<List<AdminUserAccountDto>> GetManagedAccountsAsync(bool includeDeactivated)
        {
            var users = await _userRepo.GetUsersWithDetailsAsync(includeDeactivated);
            return users.Select(MapToAdminUserAccount).ToList();
        }

        public async Task<AdminUserAccountDto> CreateAccountAsync(AdminCreateUserRequestDto request)
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var emailExists = await _userRepo.GetByEmailAsync(normalizedEmail);
            if (emailExists != null)
            {
                throw BusinessErrors.BadRequest("Email already exists.");
            }

            int? normalizedBranchId = request.BranchId;
            if (normalizedBranchId.HasValue && normalizedBranchId.Value <= 0)
            {
                normalizedBranchId = null;
            }

            if (normalizedBranchId.HasValue)
            {
                var branchExists = await _userRepo.BranchExistsAsync(normalizedBranchId.Value);
                if (!branchExists)
                {
                    throw BusinessErrors.BadRequest("Branch does not exist.");
                }
            }

            var roleName = request.Role?.Trim();
            if (string.IsNullOrWhiteSpace(roleName))
            {
                throw BusinessErrors.BadRequest("Role is required.");
            }

            var role = await _userRepo.GetRoleByNameAsync(roleName);
            if (role == null)
            {
                throw BusinessErrors.BadRequest($"Invalid role '{roleName}'.");
            }

            var now = DateTime.UtcNow;
            var user = new User
            {
                Name = request.Name.Trim(),
                Email = normalizedEmail,
                Phone = string.IsNullOrWhiteSpace(request.Phone) ? null : request.Phone.Trim(),
                BranchId = normalizedBranchId,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
                EmailVerified = false,
                CreatedAt = now,
                UpdatedAt = now,
                DeletedAt = null
            };

            user.Roles.Add(role);
            await _userRepo.AddAsync(user);

            var created = await _userRepo.GetByIdWithDetailsAsync(user.Id)
                ?? throw BusinessErrors.NotFound("Created account was not found.");

            return MapToAdminUserAccount(created);
        }

        public async Task<AdminUserAccountDto> UpdateAccountStatusAsync(int userId, bool isActive)
        {
            var user = await _userRepo.GetByIdWithDetailsAsync(userId);
            if (user == null)
            {
                throw BusinessErrors.NotFound("User not found.");
            }

            var now = DateTime.UtcNow;
            user.DeletedAt = isActive ? null : now;
            user.UpdatedAt = now;
            await _userRepo.SaveChangesAsync();

            return MapToAdminUserAccount(user);
        }

        private static AdminUserAccountDto MapToAdminUserAccount(User user)
        {
            return new AdminUserAccountDto
            {
                Id = user.Id,
                Name = user.Name,
                Email = user.Email,
                Phone = user.Phone,
                BranchId = user.BranchId,
                BranchName = user.Branch?.Name,
                EmailVerified = user.EmailVerified ?? false,
                IsActive = user.DeletedAt == null,
                Roles = user.Roles
                    .Where(r => !string.IsNullOrWhiteSpace(r.Name))
                    .Select(r => r.Name!)
                    .ToList(),
                CreatedAt = user.CreatedAt,
                LastLogin = user.LastLogin
            };
        }
    }
}

