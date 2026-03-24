using Data.Repositories.Drivers.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models.DTO.Drivers;
using Models.Models;
using Service.Services.Common;
using Service.Services.Drivers.Interfaces;

namespace Service.Services.Drivers.Implementations;

public sealed class DriverService : IDriverService
{
    private static readonly HashSet<string> GlobalRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "executivemanagement"
    };

    private static readonly HashSet<string> BranchRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "operator",
        "branchassetaccountant"
    };

    private readonly IDriverRepository _repository;
    private readonly CarManagerContext _context;

    public DriverService(IDriverRepository repository, CarManagerContext context)
    {
        _repository = repository;
        _context = context;
    }

    public async Task<ServiceResult<List<DriverResponseDto>>> GetAllAsync(int actorUserId, IReadOnlyCollection<string> roles, int? branchId)
    {
        var scope = await ResolveScopeAsync(actorUserId, roles);
        if (!scope.Success) return ServiceResult<List<DriverResponseDto>>.Fail(scope.StatusCode, scope.Message!);

        if (scope.Data!.HasValue)
        {
            if (branchId.HasValue && branchId.Value != scope.Data.Value)
                return ServiceResult<List<DriverResponseDto>>.Fail(403, "You can only access drivers in your branch.");
            branchId = scope.Data.Value;
        }

        var data = await _repository.GetAllAsync(branchId);
        return ServiceResult<List<DriverResponseDto>>.SuccessResult(data);
    }

    public async Task<ServiceResult<DriverResponseDto>> GetByIdAsync(int actorUserId, IReadOnlyCollection<string> roles, int id)
    {
        var scope = await ResolveScopeAsync(actorUserId, roles);
        if (!scope.Success) return ServiceResult<DriverResponseDto>.Fail(scope.StatusCode, scope.Message!);

        var dto = await _repository.GetByIdAsync(id);
        if (dto == null) return ServiceResult<DriverResponseDto>.Fail(404, "Driver not found.");

        if (scope.Data!.HasValue && dto.BranchId != scope.Data.Value)
            return ServiceResult<DriverResponseDto>.Fail(403, "You can only access drivers in your branch.");

        return ServiceResult<DriverResponseDto>.SuccessResult(dto);
    }

    public async Task<ServiceResult<DriverResponseDto>> CreateAsync(int actorUserId, IReadOnlyCollection<string> roles, DriverCreateDto request)
    {
        var scope = await ResolveScopeAsync(actorUserId, roles);
        if (!scope.Success) return ServiceResult<DriverResponseDto>.Fail(scope.StatusCode, scope.Message!);
        if (request == null) return ServiceResult<DriverResponseDto>.Fail(400, "Request body is required.");

        var name = request.Name?.Trim();
        var license = request.LicenseNumber?.Trim();
        if (string.IsNullOrWhiteSpace(name)) return ServiceResult<DriverResponseDto>.Fail(400, "Name is required.");
        if (string.IsNullOrWhiteSpace(license)) return ServiceResult<DriverResponseDto>.Fail(400, "LicenseNumber is required.");

        var targetBranchId = scope.Data!.HasValue ? scope.Data.Value : request.BranchId;
        if (targetBranchId <= 0 || !await _repository.BranchExistsAsync(targetBranchId))
            return ServiceResult<DriverResponseDto>.Fail(400, "BranchId not found.");

        if (await _repository.LicenseNumberExistsAsync(license, null))
            return ServiceResult<DriverResponseDto>.Fail(409, "LicenseNumber already exists.");

        var created = await _repository.CreateAsync(new Driver
        {
            Name = name,
            LicenseNumber = license,
            Phone = request.Phone?.Trim(),
            HireDate = request.HireDate,
            BranchId = targetBranchId,
            Status = "Active"
        });

        var dto = await _repository.GetByIdAsync(created.Id);
        return ServiceResult<DriverResponseDto>.SuccessResult(dto!, 201);
    }

    public async Task<ServiceResult<DriverResponseDto>> UpdateAsync(int actorUserId, IReadOnlyCollection<string> roles, int id, DriverUpdateDto request)
    {
        var scope = await ResolveScopeAsync(actorUserId, roles);
        if (!scope.Success) return ServiceResult<DriverResponseDto>.Fail(scope.StatusCode, scope.Message!);
        if (request == null) return ServiceResult<DriverResponseDto>.Fail(400, "Request body is required.");

        var entity = await _repository.GetEntityByIdAsync(id);
        if (entity == null) return ServiceResult<DriverResponseDto>.Fail(404, "Driver not found.");
        if (scope.Data!.HasValue && entity.BranchId != scope.Data.Value)
            return ServiceResult<DriverResponseDto>.Fail(403, "You can only update drivers in your branch.");

        if (!string.IsNullOrWhiteSpace(request.Name)) entity.Name = request.Name.Trim();
        if (!string.IsNullOrWhiteSpace(request.Phone)) entity.Phone = request.Phone.Trim();
        if (request.HireDate.HasValue) entity.HireDate = request.HireDate.Value;
        if (!string.IsNullOrWhiteSpace(request.Status)) entity.Status = request.Status.Trim();

        if (!string.IsNullOrWhiteSpace(request.LicenseNumber))
        {
            var license = request.LicenseNumber.Trim();
            if (await _repository.LicenseNumberExistsAsync(license, id))
                return ServiceResult<DriverResponseDto>.Fail(409, "LicenseNumber already exists.");
            entity.LicenseNumber = license;
        }

        if (request.BranchId.HasValue)
        {
            var targetBranchId = scope.Data!.HasValue ? scope.Data.Value : request.BranchId.Value;
            if (targetBranchId <= 0 || !await _repository.BranchExistsAsync(targetBranchId))
                return ServiceResult<DriverResponseDto>.Fail(400, "BranchId not found.");
            entity.BranchId = targetBranchId;
        }

        entity.UpdatedAt = DateTime.Now;
        await _repository.UpdateAsync(entity);
        var dto = await _repository.GetByIdAsync(entity.Id);
        return ServiceResult<DriverResponseDto>.SuccessResult(dto!);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int actorUserId, IReadOnlyCollection<string> roles, int id)
    {
        var scope = await ResolveScopeAsync(actorUserId, roles);
        if (!scope.Success) return ServiceResult<bool>.Fail(scope.StatusCode, scope.Message!);

        var entity = await _repository.GetEntityByIdAsync(id);
        if (entity == null) return ServiceResult<bool>.Fail(404, "Driver not found.");
        if (scope.Data!.HasValue && entity.BranchId != scope.Data.Value)
            return ServiceResult<bool>.Fail(403, "You can only delete drivers in your branch.");

        var isAssigned = await _context.Vehicles.AnyAsync(v => v.CurrentDriverId == id && v.DeletedAt == null);
        if (isAssigned) return ServiceResult<bool>.Fail(409, "Driver is currently assigned to a vehicle.");

        var deleted = await _repository.SoftDeleteAsync(id);
        return deleted
            ? ServiceResult<bool>.SuccessResult(true)
            : ServiceResult<bool>.Fail(404, "Driver not found.");
    }

    public async Task<ServiceResult<List<DriverDropdownDto>>> GetAvailableByBranchAsync(int actorUserId, IReadOnlyCollection<string> roles, int? branchId)
    {
        var scope = await ResolveScopeAsync(actorUserId, roles);
        if (!scope.Success) return ServiceResult<List<DriverDropdownDto>>.Fail(scope.StatusCode, scope.Message!);

        var targetBranchId = scope.Data!.HasValue ? scope.Data.Value : branchId.GetValueOrDefault();
        if (targetBranchId <= 0) return ServiceResult<List<DriverDropdownDto>>.Fail(400, "branchId is required.");
        if (scope.Data.HasValue && branchId.HasValue && branchId.Value != scope.Data.Value)
            return ServiceResult<List<DriverDropdownDto>>.Fail(403, "You can only access drivers in your branch.");

        var data = await _repository.GetAvailableByBranchAsync(targetBranchId);
        return ServiceResult<List<DriverDropdownDto>>.SuccessResult(data);
    }

    public async Task<ServiceResult<List<DriverDropdownDto>>> GetDropdownAsync(int actorUserId, IReadOnlyCollection<string> roles, int? branchId)
    {
        var scope = await ResolveScopeAsync(actorUserId, roles);
        if (!scope.Success) return ServiceResult<List<DriverDropdownDto>>.Fail(scope.StatusCode, scope.Message!);

        if (scope.Data!.HasValue)
        {
            if (branchId.HasValue && branchId.Value != scope.Data.Value)
                return ServiceResult<List<DriverDropdownDto>>.Fail(403, "You can only access drivers in your branch.");
            branchId = scope.Data.Value;
        }

        var data = await _repository.GetDropdownAsync(branchId);
        return ServiceResult<List<DriverDropdownDto>>.SuccessResult(data);
    }

    private async Task<ServiceResult<int?>> ResolveScopeAsync(int actorUserId, IReadOnlyCollection<string> roles)
    {
        if (roles == null || roles.Count == 0)
            return ServiceResult<int?>.Fail(403, "Role is required.");

        var normalized = roles
            .Where(r => !string.IsNullOrWhiteSpace(r))
            .Select(NormalizeRole)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (normalized.Overlaps(GlobalRoles))
            return ServiceResult<int?>.SuccessResult(null);

        if (normalized.Overlaps(BranchRoles))
        {
            var branchId = await _repository.GetUserBranchIdAsync(actorUserId);
            if (!branchId.HasValue)
                return ServiceResult<int?>.Fail(400, "User is not assigned to a branch.");
            return ServiceResult<int?>.SuccessResult(branchId.Value);
        }

        return ServiceResult<int?>.Fail(403, "You do not have permission to manage drivers.");
    }

    private static string NormalizeRole(string role)
    {
        return role.Replace(" ", string.Empty)
            .Replace("_", string.Empty)
            .Replace("-", string.Empty)
            .Trim()
            .ToLowerInvariant();
    }
}
