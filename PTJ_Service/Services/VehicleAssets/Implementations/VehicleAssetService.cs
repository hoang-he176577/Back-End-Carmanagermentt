using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Data.Repositories.VehicleAssets.Interfaces;
using Models.DTO.Vehicles;
using Models.Models;
using Service.Services.Common;
using Service.Services.VehicleAssets.Interfaces;

namespace Service.Services.VehicleAssets.Implementations;

public sealed class VehicleAssetService : IVehicleAssetService
{
    private static readonly HashSet<string> GlobalRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "executivemanagement"
    };

    private static readonly HashSet<string> BranchManageRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "branchassetaccountant",
        "operator"
    };

    private readonly IVehicleAssetRepository _repository;

    public VehicleAssetService(IVehicleAssetRepository repository)
    {
        _repository = repository;
    }

    public async Task<ServiceResult<List<VehicleAssetDto>>> GetVehiclesAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        int? branchId,
        string? status,
        bool includeDeleted)
    {
        var access = await ResolveAccessScopeAsync(actorUserId, roles, requireManagePermission: false);
        if (!access.Success)
        {
            return ServiceResult<List<VehicleAssetDto>>.Fail(access.StatusCode, access.Message!);
        }

        var scope = access.Data!;
        if (scope.RestrictedBranchId.HasValue)
        {
            if (branchId.HasValue && branchId.Value != scope.RestrictedBranchId.Value)
            {
                return ServiceResult<List<VehicleAssetDto>>.Fail(403, "You can only access assets in your branch.");
            }

            branchId = scope.RestrictedBranchId.Value;
        }

        var vehicles = await _repository.GetVehiclesAsync(branchId, status, includeDeleted);
        return ServiceResult<List<VehicleAssetDto>>.SuccessResult(vehicles);
    }

    public async Task<ServiceResult<VehicleAssetDto>> GetVehicleByIdAsync(int actorUserId, IReadOnlyCollection<string> roles, int id)
    {
        var access = await ResolveAccessScopeAsync(actorUserId, roles, requireManagePermission: false);
        if (!access.Success)
        {
            return ServiceResult<VehicleAssetDto>.Fail(access.StatusCode, access.Message!);
        }

        var vehicle = await _repository.GetVehicleByIdAsync(id);
        if (vehicle == null)
        {
            return ServiceResult<VehicleAssetDto>.Fail(404, "Vehicle not found.");
        }

        var scope = access.Data!;
        if (scope.RestrictedBranchId.HasValue && vehicle.CurrentBranchId != scope.RestrictedBranchId.Value)
        {
            return ServiceResult<VehicleAssetDto>.Fail(403, "You can only access assets in your branch.");
        }

        return ServiceResult<VehicleAssetDto>.SuccessResult(vehicle);
    }

    public async Task<ServiceResult<VehicleAssetDto>> CreateVehicleAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        VehicleCreateRequestDto request)
    {
        var access = await ResolveAccessScopeAsync(actorUserId, roles, requireManagePermission: true);
        if (!access.Success)
        {
            return ServiceResult<VehicleAssetDto>.Fail(access.StatusCode, access.Message!);
        }

        if (request == null)
        {
            return ServiceResult<VehicleAssetDto>.Fail(400, "Request body is required.");
        }

        var licensePlate = request.LicensePlate?.Trim();
        if (string.IsNullOrWhiteSpace(licensePlate))
        {
            return ServiceResult<VehicleAssetDto>.Fail(400, "LicensePlate is required.");
        }

        if (request.ModelId is null || request.ModelId <= 0)
        {
            return ServiceResult<VehicleAssetDto>.Fail(400, "ModelId is required.");
        }

        var commonValidation = await ValidateCommonInputsAsync(
            request.ModelId,
            request.CurrentDriverId,
            request.OriginalCost,
            request.CurrentValue,
            request.Mileage,
            requestId: null,
            licensePlate);

        if (!commonValidation.Success)
        {
            return ServiceResult<VehicleAssetDto>.Fail(commonValidation.StatusCode, commonValidation.Message!);
        }

        var scope = access.Data!;
        int? targetBranchId = null;
        if (scope.RestrictedBranchId.HasValue)
        {
            // Branch-scoped users always create assets in their own branch.
            targetBranchId = scope.RestrictedBranchId.Value;
        }

        var status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status.Trim();
        var currentValue = request.CurrentValue ?? request.OriginalCost;
        var mileage = request.Mileage ?? 0m;

        var vehicle = new Vehicle
        {
            LicensePlate = licensePlate,
            ModelId = request.ModelId,
            YearManufacture = request.YearManufacture,
            PurchaseDate = request.PurchaseDate,
            OriginalCost = request.OriginalCost,
            CurrentValue = currentValue,
            Mileage = mileage,
            Status = status,
            CurrentBranchId = targetBranchId,
            CurrentDriverId = request.CurrentDriverId
        };

        var created = await _repository.AddVehicleAsync(vehicle);
        var createdDto = await _repository.GetVehicleByIdAsync(created.Id);
        if (createdDto == null)
        {
            return ServiceResult<VehicleAssetDto>.Fail(500, "Failed to load created vehicle.");
        }

        return ServiceResult<VehicleAssetDto>.SuccessResult(createdDto, 201);
    }

    public async Task<ServiceResult<VehicleAssetDto>> UpdateVehicleAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        int id,
        VehicleUpdateRequestDto request)
    {
        var access = await ResolveAccessScopeAsync(actorUserId, roles, requireManagePermission: true);
        if (!access.Success)
        {
            return ServiceResult<VehicleAssetDto>.Fail(access.StatusCode, access.Message!);
        }

        if (request == null)
        {
            return ServiceResult<VehicleAssetDto>.Fail(400, "Request body is required.");
        }

        var vehicle = await _repository.GetVehicleEntityByIdAsync(id);
        if (vehicle == null)
        {
            return ServiceResult<VehicleAssetDto>.Fail(404, "Vehicle not found.");
        }

        var scope = access.Data!;
        if (scope.RestrictedBranchId.HasValue && vehicle.CurrentBranchId != scope.RestrictedBranchId.Value)
        {
            return ServiceResult<VehicleAssetDto>.Fail(403, "You can only update assets in your branch.");
        }

        var incomingLicense = request.LicensePlate?.Trim();
        if (!string.IsNullOrWhiteSpace(incomingLicense))
        {
            var duplicate = await _repository.LicensePlateExistsForOtherVehicleAsync(incomingLicense, id);
            if (duplicate)
            {
                return ServiceResult<VehicleAssetDto>.Fail(409, "LicensePlate already exists.");
            }

            vehicle.LicensePlate = incomingLicense;
        }

        if (request.ModelId.HasValue)
        {
            if (request.ModelId.Value <= 0 || !await _repository.ModelExistsAsync(request.ModelId.Value))
            {
                return ServiceResult<VehicleAssetDto>.Fail(400, "ModelId not found.");
            }

            vehicle.ModelId = request.ModelId.Value;
        }

        if (scope.RestrictedBranchId.HasValue)
        {
            // Keep branch locked to the actor's branch for branch-scoped users.
            vehicle.CurrentBranchId = scope.RestrictedBranchId.Value;
        }

        if (request.CurrentDriverId.HasValue)
        {
            if (!await _repository.DriverExistsAsync(request.CurrentDriverId.Value))
            {
                return ServiceResult<VehicleAssetDto>.Fail(400, "CurrentDriverId not found.");
            }

            vehicle.CurrentDriverId = request.CurrentDriverId.Value;
        }

        if (request.OriginalCost.HasValue)
        {
            if (request.OriginalCost.Value < 0)
            {
                return ServiceResult<VehicleAssetDto>.Fail(400, "OriginalCost must be >= 0.");
            }

            vehicle.OriginalCost = request.OriginalCost.Value;
        }

        if (request.CurrentValue.HasValue)
        {
            if (request.CurrentValue.Value < 0)
            {
                return ServiceResult<VehicleAssetDto>.Fail(400, "CurrentValue must be >= 0.");
            }

            vehicle.CurrentValue = request.CurrentValue.Value;
        }

        if (request.Mileage.HasValue)
        {
            if (request.Mileage.Value < 0)
            {
                return ServiceResult<VehicleAssetDto>.Fail(400, "Mileage must be >= 0.");
            }

            vehicle.Mileage = request.Mileage.Value;
        }

        if (request.Status != null)
        {
            vehicle.Status = request.Status.Trim();
        }

        if (request.YearManufacture.HasValue)
        {
            vehicle.YearManufacture = request.YearManufacture.Value;
        }

        if (request.PurchaseDate.HasValue)
        {
            vehicle.PurchaseDate = request.PurchaseDate.Value;
        }

        vehicle.UpdatedAt = DateTime.Now;
        await _repository.SaveChangesAsync();

        var updated = await _repository.GetVehicleByIdAsync(vehicle.Id);
        if (updated == null)
        {
            return ServiceResult<VehicleAssetDto>.Fail(500, "Failed to load updated vehicle.");
        }

        return ServiceResult<VehicleAssetDto>.SuccessResult(updated);
    }

    private async Task<ServiceResult<bool>> ValidateCommonInputsAsync(
        int? modelId,
        int? currentDriverId,
        decimal? originalCost,
        decimal? currentValue,
        decimal? mileage,
        int? requestId,
        string licensePlate)
    {
        if (originalCost.HasValue && originalCost.Value < 0)
        {
            return ServiceResult<bool>.Fail(400, "OriginalCost must be >= 0.");
        }

        if (currentValue.HasValue && currentValue.Value < 0)
        {
            return ServiceResult<bool>.Fail(400, "CurrentValue must be >= 0.");
        }

        if (mileage.HasValue && mileage.Value < 0)
        {
            return ServiceResult<bool>.Fail(400, "Mileage must be >= 0.");
        }

        if (requestId.HasValue)
        {
            var duplicate = await _repository.LicensePlateExistsForOtherVehicleAsync(licensePlate, requestId.Value);
            if (duplicate)
            {
                return ServiceResult<bool>.Fail(409, "LicensePlate already exists.");
            }
        }
        else
        {
            var exists = await _repository.LicensePlateExistsAsync(licensePlate);
            if (exists)
            {
                return ServiceResult<bool>.Fail(409, "LicensePlate already exists.");
            }
        }

        if (!modelId.HasValue || !await _repository.ModelExistsAsync(modelId.Value))
        {
            return ServiceResult<bool>.Fail(400, "ModelId not found.");
        }

        if (currentDriverId.HasValue && !await _repository.DriverExistsAsync(currentDriverId.Value))
        {
            return ServiceResult<bool>.Fail(400, "CurrentDriverId not found.");
        }

        return ServiceResult<bool>.SuccessResult(true);
    }

    private async Task<ServiceResult<AccessScope>> ResolveAccessScopeAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        bool requireManagePermission)
    {
        if (roles == null || roles.Count == 0)
        {
            return ServiceResult<AccessScope>.Fail(403, "Role is required.");
        }

        var normalized = roles
            .Where(role => !string.IsNullOrWhiteSpace(role))
            .Select(NormalizeRole)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (normalized.Overlaps(GlobalRoles))
        {
            return ServiceResult<AccessScope>.SuccessResult(new AccessScope(null, true));
        }

        if (normalized.Overlaps(BranchManageRoles))
        {
            var userBranchId = await _repository.GetUserBranchIdAsync(actorUserId);
            if (!userBranchId.HasValue)
            {
                return ServiceResult<AccessScope>.Fail(400, "User is not assigned to a branch.");
            }

            return ServiceResult<AccessScope>.SuccessResult(new AccessScope(userBranchId.Value, true));
        }

        return ServiceResult<AccessScope>.Fail(403, "You do not have permission to manage vehicle assets.");
    }

    private static string NormalizeRole(string role)
    {
        return role.Replace(" ", string.Empty)
            .Replace("_", string.Empty)
            .Replace("-", string.Empty)
            .Trim()
            .ToLowerInvariant();
    }

    private sealed record AccessScope(int? RestrictedBranchId, bool CanManage);

    // ===== DROPDOWN DATA METHODS =====

    public async Task<List<VehicleModelDto>> GetVehicleModelsAsync()
    {
        return await _repository.GetVehicleModelsAsync();
    }

    public async Task<List<BranchDto>> GetBranchesAsync()
    {
        return await _repository.GetBranchesAsync();
    }

    public async Task<List<DriverDto>> GetDriversAsync()
    {
        return await _repository.GetDriversAsync();
    }
}
