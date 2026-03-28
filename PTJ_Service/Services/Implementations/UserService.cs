using Data.Repositories.Auth.Interfaces;
using Models.DTO.User;
using Models.Models;
using Service.Exceptions;
using Service.Services.Auth.Interfaces;
using Service.Services.Interfaces;
using Service.Services.Interfaces.Repository;
using System.Linq;

namespace Service.Services.Implementations
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepo;
        private readonly IAuthRepository _authRepo;
        private readonly IAuthService _authService;

        private static readonly Dictionary<string, string> RoleAliasMap = new(StringComparer.OrdinalIgnoreCase)
        {
            ["branchassetaccountant"] = "Branch Asset Accountant",
            ["executivemanagement"] = "Executive Management"
        };

        private static readonly HashSet<string> ManagerAllowedRoles = new(StringComparer.OrdinalIgnoreCase)
        {
            "Operator",
            "Branch Asset Accountant",
            "BranchAssetAccountant"
        };

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
                var role = request.Role.Trim();
                var added = await AddRoleWithFallbackAsync(created.Id, role);
                if (!added)
                {
                    throw BusinessErrors.BadRequest($"Invalid role '{role}'.");
                }
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

        public async Task<List<AdminAccountDto>> GetManagerAccountsAsync(int? branchId, bool includeDeactivated, bool canViewAll)
        {
            if (!canViewAll && (!branchId.HasValue || branchId.Value <= 0))
                throw BusinessErrors.BadRequest("Branch is required.");

            var users = canViewAll
                ? (branchId.HasValue
                    ? await _userRepo.GetUsersByBranchWithBranchAsync(branchId.Value, includeDeactivated)
                    : await _userRepo.GetAllUsersWithBranchAsync(includeDeactivated))
                : await _userRepo.GetUsersByBranchWithBranchAsync(branchId!.Value, includeDeactivated);

            var result = new List<AdminAccountDto>();
            foreach (var user in users)
            {
                var roles = await _authRepo.GetUserRolesAsync(user.Id);
                if (!canViewAll && !roles.Any(r => ManagerAllowedRoles.Contains(r)))
                {
                    continue;
                }
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

        public async Task<AdminAccountDto> CreateManagerAccountAsync(CreateAdminAccountDto request, int? branchId, bool canViewAll)
        {
            int? effectiveBranchId = branchId;
            if (canViewAll)
            {
                effectiveBranchId = request.BranchId;
            }

            if (!effectiveBranchId.HasValue || effectiveBranchId.Value <= 0)
                throw BusinessErrors.BadRequest("Branch is required.");

            if (string.IsNullOrWhiteSpace(request.Email))
                throw BusinessErrors.BadRequest("Email is required.");

            if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 6)
                throw BusinessErrors.BadRequest("Password must be at least 6 characters.");

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();

            if (await _authRepo.EmailExistsAsync(normalizedEmail))
                throw BusinessErrors.Conflict("Email already exists.");

            if (!await _authRepo.BranchExistsAsync(effectiveBranchId.Value))
                throw BusinessErrors.BadRequest("Branch not found.");

            var rawRole = request.Role?.Trim();
            if (string.IsNullOrWhiteSpace(rawRole))
                throw BusinessErrors.BadRequest("Role is required.");

            var mappedRole = MapRoleAlias(rawRole);
            if (!ManagerAllowedRoles.Contains(mappedRole))
            {
                throw BusinessErrors.Unauthorized("You are not allowed to assign this role.");
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var user = new User
            {
                Name = request.Name?.Trim(),
                Email = normalizedEmail,
                PasswordHash = passwordHash,
                Phone = request.Phone?.Trim(),
                BranchId = effectiveBranchId,
                EmailVerified = false,
                CreatedAt = DateTime.UtcNow
            };

            var created = await _authRepo.CreateUserAsync(user);

            var added = await AddRoleWithFallbackAsync(created.Id, rawRole);
            if (!added)
            {
                throw BusinessErrors.BadRequest($"Invalid role '{rawRole}'.");
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

        public async Task UpdateAccountStatusForManagerAsync(int id, bool isActive, int? branchId, bool canViewAll)
        {
            if (!canViewAll && (!branchId.HasValue || branchId.Value <= 0))
                throw BusinessErrors.BadRequest("Branch is required.");

            var user = await _userRepo.GetByIdWithBranchAsync(id);
            if (user == null)
                throw BusinessErrors.NotFound("User not found.");

            if (!canViewAll && user.BranchId != branchId)
                throw BusinessErrors.Unauthorized("You are not allowed to manage this account.");

            if (!canViewAll)
            {
                var roles = await _authRepo.GetUserRolesAsync(user.Id);
                if (!roles.Any(r => ManagerAllowedRoles.Contains(r)))
                    throw BusinessErrors.Unauthorized("You are not allowed to manage this account.");
            }

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

        private static string MapRoleAlias(string role)
        {
            var key = NormalizeRoleKey(role);
            return RoleAliasMap.TryGetValue(key, out var mapped) ? mapped : role;
        }

        private static string NormalizeRoleKey(string role)
        {
            var chars = role.Where(char.IsLetterOrDigit).ToArray();
            return new string(chars).ToLowerInvariant();
        }

        private async Task<bool> AddRoleWithFallbackAsync(int userId, string role)
        {
            var canonical = MapRoleAlias(role);
            if (await _authRepo.AddRoleToUserAsync(userId, canonical))
            {
                return true;
            }

            if (!string.Equals(canonical, role, StringComparison.OrdinalIgnoreCase))
            {
                return await _authRepo.AddRoleToUserAsync(userId, role);
            }

            return false;
        }
    }
}

