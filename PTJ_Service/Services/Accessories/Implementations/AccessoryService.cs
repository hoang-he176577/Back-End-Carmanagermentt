using Data.Repositories.Accessories.Interfaces;
using Models.DTO.Accessories;
using Models.Models;
using Service.Services.Accessories.Interfaces;
using Service.Services.Common;

namespace Service.Services.Accessories.Implementations;

public sealed class AccessoryService : IAccessoryService
{
    private const string DisposedVehicleStatus = "Disposed";
    private const string LiquidatedVehicleStatus = "Liquidated";

    private static readonly HashSet<string> GlobalRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "executivemanagement"
    };

    private static readonly HashSet<string> BranchRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "operator",
        "branchassetaccountant"
    };

    private static readonly HashSet<string> WriteRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "operator",
        "executivemanagement"
    };

    private static readonly HashSet<string> AllowedAccessoryTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Reusable",
        "Consumable",
        "Fixed"
    };

    private static readonly HashSet<string> AllowedVehicleAccessoryStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Installed",
        "Returned",
        "Damaged",
        "Lost",
        "Removed"
    };

    private static readonly HashSet<string> AllowedTransactionTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "IMPORT",
        "ISSUE",
        "RETURN",
        "DAMAGED",
        "LOST",
        "ADJUST"
    };

    private readonly IAccessoryRepository _repository;

    public AccessoryService(IAccessoryRepository repository)
    {
        _repository = repository;
    }

    public async Task<ServiceResult<List<AccessoryDto>>> GetAccessoriesAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        string? keyword,
        string? type,
        bool? isActive,
        int? page,
        int? pageSize)
    {
        var access = await ResolveAccessScopeAsync(actorUserId, roles, requireWritePermission: false);
        if (!access.Success)
        {
            return ServiceResult<List<AccessoryDto>>.Fail(access.StatusCode, access.Message!);
        }

        if (!string.IsNullOrWhiteSpace(type) && !AllowedAccessoryTypes.Contains(type.Trim()))
        {
            return ServiceResult<List<AccessoryDto>>.Fail(400, "Invalid accessory type.");
        }

        var items = await _repository.GetAccessoriesAsync(
            keyword,
            string.IsNullOrWhiteSpace(type) ? null : NormalizeAccessoryType(type),
            isActive,
            page,
            pageSize);

        return ServiceResult<List<AccessoryDto>>.SuccessResult(items);
    }

    public async Task<ServiceResult<AccessoryDto>> GetAccessoryByIdAsync(int actorUserId, IReadOnlyCollection<string> roles, int id)
    {
        var access = await ResolveAccessScopeAsync(actorUserId, roles, requireWritePermission: false);
        if (!access.Success)
        {
            return ServiceResult<AccessoryDto>.Fail(access.StatusCode, access.Message!);
        }

        var item = await _repository.GetAccessoryByIdAsync(id);
        if (item == null)
        {
            return ServiceResult<AccessoryDto>.Fail(404, "Accessory not found.");
        }

        return ServiceResult<AccessoryDto>.SuccessResult(item);
    }

    public async Task<ServiceResult<AccessoryDto>> CreateAccessoryAsync(int actorUserId, IReadOnlyCollection<string> roles, AccessoryCreateRequestDto request)
    {
        var access = await ResolveAccessScopeAsync(actorUserId, roles, requireWritePermission: true);
        if (!access.Success)
        {
            return ServiceResult<AccessoryDto>.Fail(access.StatusCode, access.Message!);
        }

        if (request == null)
        {
            return ServiceResult<AccessoryDto>.Fail(400, "Request body is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Code))
        {
            return ServiceResult<AccessoryDto>.Fail(400, "Code is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return ServiceResult<AccessoryDto>.Fail(400, "Name is required.");
        }

        if (!AllowedAccessoryTypes.Contains(request.Type))
        {
            return ServiceResult<AccessoryDto>.Fail(400, "Type must be Reusable, Consumable, or Fixed.");
        }

        if (request.QuantityInStock < 0)
        {
            return ServiceResult<AccessoryDto>.Fail(400, "QuantityInStock must be >= 0.");
        }

        var normalizedCode = request.Code.Trim();
        if (await _repository.AccessoryCodeExistsAsync(normalizedCode))
        {
            return ServiceResult<AccessoryDto>.Fail(409, "Accessory code already exists.");
        }

        var now = DateTime.UtcNow;
        var entity = new Accessory
        {
            Code = normalizedCode,
            Name = request.Name.Trim(),
            Type = NormalizeAccessoryType(request.Type),
            QuantityInStock = request.QuantityInStock,
            UnitPrice = request.UnitPrice,
            MinimumStock = request.MinimumStock,
            IsActive = request.IsActive,
            CreatedAt = now,
            UpdatedAt = now
        };

        var created = await _repository.AddAccessoryAsync(entity);
        var dto = await _repository.GetAccessoryByIdAsync(created.Id);
        if (dto == null)
        {
            return ServiceResult<AccessoryDto>.Fail(500, "Failed to load created accessory.");
        }

        return ServiceResult<AccessoryDto>.SuccessResult(dto, 201);
    }

    public async Task<ServiceResult<AccessoryDto>> UpdateAccessoryAsync(int actorUserId, IReadOnlyCollection<string> roles, int id, AccessoryUpdateRequestDto request)
    {
        var access = await ResolveAccessScopeAsync(actorUserId, roles, requireWritePermission: true);
        if (!access.Success)
        {
            return ServiceResult<AccessoryDto>.Fail(access.StatusCode, access.Message!);
        }

        if (request == null)
        {
            return ServiceResult<AccessoryDto>.Fail(400, "Request body is required.");
        }

        var entity = await _repository.GetAccessoryEntityByIdAsync(id);
        if (entity == null)
        {
            return ServiceResult<AccessoryDto>.Fail(404, "Accessory not found.");
        }

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var normalizedCode = request.Code.Trim();
            if (await _repository.AccessoryCodeExistsAsync(normalizedCode, id))
            {
                return ServiceResult<AccessoryDto>.Fail(409, "Accessory code already exists.");
            }

            entity.Code = normalizedCode;
        }

        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            entity.Name = request.Name.Trim();
        }

        if (!string.IsNullOrWhiteSpace(request.Type))
        {
            if (!AllowedAccessoryTypes.Contains(request.Type.Trim()))
            {
                return ServiceResult<AccessoryDto>.Fail(400, "Type must be Reusable, Consumable, or Fixed.");
            }

            entity.Type = NormalizeAccessoryType(request.Type);
        }

        if (request.QuantityInStock.HasValue)
        {
            if (request.QuantityInStock.Value < 0)
            {
                return ServiceResult<AccessoryDto>.Fail(400, "QuantityInStock must be >= 0.");
            }

            entity.QuantityInStock = request.QuantityInStock.Value;
        }

        if (request.MinimumStock.HasValue && request.MinimumStock.Value < 0)
        {
            return ServiceResult<AccessoryDto>.Fail(400, "MinimumStock must be >= 0.");
        }

        if (request.UnitPrice.HasValue && request.UnitPrice.Value < 0)
        {
            return ServiceResult<AccessoryDto>.Fail(400, "UnitPrice must be >= 0.");
        }

        if (request.MinimumStock.HasValue)
        {
            entity.MinimumStock = request.MinimumStock.Value;
        }

        if (request.UnitPrice.HasValue)
        {
            entity.UnitPrice = request.UnitPrice.Value;
        }

        if (request.IsActive.HasValue)
        {
            entity.IsActive = request.IsActive.Value;
        }

        entity.UpdatedAt = DateTime.UtcNow;
        await _repository.SaveChangesAsync();

        var dto = await _repository.GetAccessoryByIdAsync(entity.Id);
        if (dto == null)
        {
            return ServiceResult<AccessoryDto>.Fail(500, "Failed to load updated accessory.");
        }

        return ServiceResult<AccessoryDto>.SuccessResult(dto);
    }

    public async Task<ServiceResult<AccessoryDto>> ImportAccessoryAsync(int actorUserId, IReadOnlyCollection<string> roles, int id, AccessoryImportRequestDto request)
    {
        var access = await ResolveAccessScopeAsync(actorUserId, roles, requireWritePermission: true);
        if (!access.Success)
        {
            return ServiceResult<AccessoryDto>.Fail(access.StatusCode, access.Message!);
        }

        if (request == null)
        {
            return ServiceResult<AccessoryDto>.Fail(400, "Request body is required.");
        }

        if (request.Quantity <= 0)
        {
            return ServiceResult<AccessoryDto>.Fail(400, "Quantity must be greater than 0.");
        }

        var accessory = await _repository.GetAccessoryEntityByIdAsync(id);
        if (accessory == null)
        {
            return ServiceResult<AccessoryDto>.Fail(404, "Accessory not found.");
        }

        accessory.QuantityInStock = (accessory.QuantityInStock ?? 0) + request.Quantity;
        accessory.UpdatedAt = DateTime.UtcNow;

        var performedBy = request.PerformedBy ?? actorUserId;

        await _repository.AddAccessoryTransactionAsync(new AccessoryTransaction
        {
            AccessoryId = accessory.Id,
            VehicleId = null,
            VehicleAccessoryId = null,
            TransactionType = "IMPORT",
            Quantity = request.Quantity,
            UnitPrice = accessory.UnitPrice,
            Notes = request.Notes,
            PerformedBy = performedBy,
            TransactionDate = DateTime.UtcNow
        });

        await _repository.SaveChangesAsync();

        var dto = await _repository.GetAccessoryByIdAsync(accessory.Id);
        if (dto == null)
        {
            return ServiceResult<AccessoryDto>.Fail(500, "Failed to load updated accessory.");
        }

        return ServiceResult<AccessoryDto>.SuccessResult(dto);
    }

    public async Task<ServiceResult<IssueVehicleAccessoryResponseDto>> IssueVehicleAccessoryAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        IssueVehicleAccessoryRequestDto request)
    {
        var access = await ResolveAccessScopeAsync(actorUserId, roles, requireWritePermission: true);
        if (!access.Success)
        {
            return ServiceResult<IssueVehicleAccessoryResponseDto>.Fail(access.StatusCode, access.Message!);
        }

        if (request == null)
        {
            return ServiceResult<IssueVehicleAccessoryResponseDto>.Fail(400, "Request body is required.");
        }

        if (request.Quantity <= 0)
        {
            return ServiceResult<IssueVehicleAccessoryResponseDto>.Fail(400, "Quantity must be greater than 0.");
        }

        var accessory = await _repository.GetAccessoryEntityByIdAsync(request.AccessoryId);
        if (accessory == null)
        {
            return ServiceResult<IssueVehicleAccessoryResponseDto>.Fail(404, "Accessory not found.");
        }

        var vehicle = await _repository.GetVehicleEntityByIdAsync(request.VehicleId);
        if (vehicle == null)
        {
            return ServiceResult<IssueVehicleAccessoryResponseDto>.Fail(404, "Vehicle not found.");
        }

        if (IsDisposed(vehicle.Status))
        {
            return ServiceResult<IssueVehicleAccessoryResponseDto>.Fail(400, "Disposed or liquidated vehicles cannot be assigned accessories.");
        }

        var scope = access.Data!;
        if (scope.RestrictedBranchId.HasValue && vehicle.CurrentBranchId != scope.RestrictedBranchId.Value)
        {
            return ServiceResult<IssueVehicleAccessoryResponseDto>.Fail(403, "You can only issue accessories for vehicles in your branch.");
        }

        var currentStock = accessory.QuantityInStock ?? 0;
        if (currentStock < request.Quantity)
        {
            return ServiceResult<IssueVehicleAccessoryResponseDto>.Fail(400, "Not enough stock to issue accessory.");
        }

        ServiceResult<IssueVehicleAccessoryResponseDto>? result = null;
        await _repository.ExecuteInTransactionAsync(async () =>
        {
            var now = DateTime.UtcNow;
            var installedBy = request.InstalledBy ?? actorUserId;
            var vehicleAccessory = new VehicleAccessory
            {
                VehicleId = request.VehicleId,
                AccessoryId = request.AccessoryId,
                Quantity = request.Quantity,
                InstallDate = request.InstallDate ?? DateOnly.FromDateTime(now),
                Notes = request.Notes,
                InstalledBy = installedBy,
                Status = "Installed",
                CreatedAt = now,
                UpdatedAt = now
            };

            var createdVehicleAccessory = await _repository.AddVehicleAccessoryAsync(vehicleAccessory);

            accessory.QuantityInStock = currentStock - request.Quantity;
            accessory.UpdatedAt = now;
            await _repository.SaveChangesAsync();

            await _repository.AddAccessoryTransactionAsync(new AccessoryTransaction
            {
                AccessoryId = request.AccessoryId,
                VehicleId = request.VehicleId,
                VehicleAccessoryId = createdVehicleAccessory.Id,
                TransactionType = "ISSUE",
                Quantity = request.Quantity,
                UnitPrice = accessory.UnitPrice,
                Notes = request.Notes,
                PerformedBy = installedBy,
                TransactionDate = now
            });

            var persisted = await _repository.GetVehicleAccessoryWithAccessoryAsync(createdVehicleAccessory.Id);
            if (persisted == null)
            {
                throw new InvalidOperationException("Failed to load created vehicle accessory.");
            }

            result = ServiceResult<IssueVehicleAccessoryResponseDto>.SuccessResult(new IssueVehicleAccessoryResponseDto
            {
                VehicleAccessory = MapVehicleAccessory(persisted),
                RemainingStock = accessory.QuantityInStock ?? 0
            }, 201);
        });

        return result ?? ServiceResult<IssueVehicleAccessoryResponseDto>.Fail(500, "Failed to issue accessory.");
    }

    public async Task<ServiceResult<VehicleAccessoryDto>> HandleVehicleAccessoryActionAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        int vehicleAccessoryId,
        ReturnVehicleAccessoryRequestDto request)
    {
        var access = await ResolveAccessScopeAsync(actorUserId, roles, requireWritePermission: true);
        if (!access.Success)
        {
            return ServiceResult<VehicleAccessoryDto>.Fail(access.StatusCode, access.Message!);
        }

        if (request == null)
        {
            return ServiceResult<VehicleAccessoryDto>.Fail(400, "Request body is required.");
        }

        var action = request.ActionType?.Trim();
        if (string.IsNullOrWhiteSpace(action))
        {
            return ServiceResult<VehicleAccessoryDto>.Fail(400, "ActionType is required.");
        }

        action = action.ToUpperInvariant();
        if (action is not ("RETURN" or "DAMAGED" or "LOST"))
        {
            return ServiceResult<VehicleAccessoryDto>.Fail(400, "ActionType must be RETURN, DAMAGED, or LOST.");
        }

        var entity = await _repository.GetVehicleAccessoryWithAccessoryAsync(vehicleAccessoryId);
        if (entity == null)
        {
            return ServiceResult<VehicleAccessoryDto>.Fail(404, "Vehicle accessory not found.");
        }

        if (!AllowedVehicleAccessoryStatuses.Contains(entity.Status))
        {
            return ServiceResult<VehicleAccessoryDto>.Fail(400, "Vehicle accessory has invalid status.");
        }

        if (!string.Equals(entity.Status, "Installed", StringComparison.OrdinalIgnoreCase))
        {
            return ServiceResult<VehicleAccessoryDto>.Fail(400, "Only Installed records can be processed.");
        }

        if (entity.Accessory == null)
        {
            return ServiceResult<VehicleAccessoryDto>.Fail(404, "Accessory not found.");
        }

        var scope = access.Data!;
        if (scope.RestrictedBranchId.HasValue && entity.VehicleId.HasValue)
        {
            var vehicleBranchId = await _repository.GetVehicleBranchIdAsync(entity.VehicleId.Value);
            if (vehicleBranchId != scope.RestrictedBranchId.Value)
            {
                return ServiceResult<VehicleAccessoryDto>.Fail(403, "You can only process records in your branch.");
            }
        }

        ServiceResult<VehicleAccessoryDto>? result = null;
        await _repository.ExecuteInTransactionAsync(async () =>
        {
            var now = DateTime.UtcNow;
            var removedBy = request.RemovedBy ?? actorUserId;
            entity.RemoveDate = request.RemoveDate ?? DateOnly.FromDateTime(now);
            entity.RemovedBy = removedBy;
            entity.Notes = request.Notes;
            entity.UpdatedAt = now;

            var transactionType = action switch
            {
                "RETURN" => "RETURN",
                "DAMAGED" => "DAMAGED",
                "LOST" => "LOST",
                _ => action
            };

            entity.Status = action switch
            {
                "RETURN" => "Returned",
                "DAMAGED" => "Damaged",
                "LOST" => "Lost",
                _ => entity.Status
            };

            if (action == "RETURN")
            {
                entity.Accessory.QuantityInStock = (entity.Accessory.QuantityInStock ?? 0) + entity.Quantity;
            }

            entity.Accessory.UpdatedAt = now;
            await _repository.SaveChangesAsync();

            await _repository.AddAccessoryTransactionAsync(new AccessoryTransaction
            {
                AccessoryId = entity.AccessoryId ?? entity.Accessory.Id,
                VehicleId = entity.VehicleId,
                VehicleAccessoryId = entity.Id,
                TransactionType = transactionType,
                Quantity = entity.Quantity,
                UnitPrice = entity.Accessory.UnitPrice,
                Notes = request.Notes,
                PerformedBy = removedBy,
                TransactionDate = now
            });

            result = ServiceResult<VehicleAccessoryDto>.SuccessResult(MapVehicleAccessory(entity));
        });

        return result ?? ServiceResult<VehicleAccessoryDto>.Fail(500, "Failed to process vehicle accessory action.");
    }

    public async Task<ServiceResult<List<VehicleAccessoryDto>>> GetVehicleAccessoriesAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        int vehicleId,
        bool activeOnly)
    {
        var access = await ResolveAccessScopeAsync(actorUserId, roles, requireWritePermission: false);
        if (!access.Success)
        {
            return ServiceResult<List<VehicleAccessoryDto>>.Fail(access.StatusCode, access.Message!);
        }

        var vehicle = await _repository.GetVehicleEntityByIdAsync(vehicleId);
        if (vehicle == null)
        {
            return ServiceResult<List<VehicleAccessoryDto>>.Fail(404, "Vehicle not found.");
        }

        var scope = access.Data!;
        if (scope.RestrictedBranchId.HasValue && vehicle.CurrentBranchId != scope.RestrictedBranchId.Value)
        {
            return ServiceResult<List<VehicleAccessoryDto>>.Fail(403, "You can only view vehicles in your branch.");
        }

        var items = await _repository.GetVehicleAccessoriesByVehicleIdAsync(vehicleId, activeOnly);
        return ServiceResult<List<VehicleAccessoryDto>>.SuccessResult(items);
    }

    public async Task<ServiceResult<List<AccessoryTransactionDto>>> GetTransactionsAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        int? accessoryId,
        int? vehicleId,
        string? transactionType,
        DateTime? fromDate,
        DateTime? toDate,
        int? page,
        int? pageSize)
    {
        var access = await ResolveAccessScopeAsync(actorUserId, roles, requireWritePermission: false);
        if (!access.Success)
        {
            return ServiceResult<List<AccessoryTransactionDto>>.Fail(access.StatusCode, access.Message!);
        }

        if (!string.IsNullOrWhiteSpace(transactionType) && !AllowedTransactionTypes.Contains(transactionType.Trim()))
        {
            return ServiceResult<List<AccessoryTransactionDto>>.Fail(400, "Invalid transaction type.");
        }

        if (fromDate.HasValue && toDate.HasValue && fromDate.Value > toDate.Value)
        {
            return ServiceResult<List<AccessoryTransactionDto>>.Fail(400, "fromDate must be less than or equal to toDate.");
        }

        var items = await _repository.GetAccessoryTransactionsAsync(
            accessoryId,
            vehicleId,
            string.IsNullOrWhiteSpace(transactionType) ? null : NormalizeTransactionType(transactionType),
            fromDate,
            toDate,
            access.Data!.RestrictedBranchId,
            page,
            pageSize);

        return ServiceResult<List<AccessoryTransactionDto>>.SuccessResult(items);
    }

    private static string NormalizeAccessoryType(string type)
        => AllowedAccessoryTypes.First(x => x.Equals(type.Trim(), StringComparison.OrdinalIgnoreCase));

    private static string NormalizeTransactionType(string transactionType)
        => AllowedTransactionTypes.First(x => x.Equals(transactionType.Trim(), StringComparison.OrdinalIgnoreCase));

    private static VehicleAccessoryDto MapVehicleAccessory(VehicleAccessory entity)
    {
        return new VehicleAccessoryDto
        {
            Id = entity.Id,
            VehicleId = entity.VehicleId,
            AccessoryId = entity.AccessoryId,
            AccessoryCode = entity.Accessory?.Code,
            AccessoryName = entity.Accessory?.Name,
            AccessoryType = entity.Accessory?.Type,
            Quantity = entity.Quantity,
            Status = entity.Status,
            InstallDate = entity.InstallDate,
            RemoveDate = entity.RemoveDate,
            Notes = entity.Notes,
            InstalledBy = entity.InstalledBy,
            RemovedBy = entity.RemovedBy,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }

    private async Task<ServiceResult<AccessScope>> ResolveAccessScopeAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        bool requireWritePermission)
    {
        if (roles == null || roles.Count == 0)
        {
            return ServiceResult<AccessScope>.Fail(403, "Role is required.");
        }

        var normalizedRoles = roles
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(NormalizeRole)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (requireWritePermission && !normalizedRoles.Overlaps(WriteRoles))
        {
            return ServiceResult<AccessScope>.Fail(403, "You do not have permission to perform this action.");
        }

        if (!requireWritePermission &&
            !normalizedRoles.Overlaps(GlobalRoles) &&
            !normalizedRoles.Overlaps(BranchRoles))
        {
            return ServiceResult<AccessScope>.Fail(403, "You do not have permission to access accessory data.");
        }

        if (normalizedRoles.Overlaps(GlobalRoles))
        {
            return ServiceResult<AccessScope>.SuccessResult(new AccessScope(null));
        }

        var userBranchId = await _repository.GetUserBranchIdAsync(actorUserId);
        if (!userBranchId.HasValue)
        {
            return ServiceResult<AccessScope>.Fail(400, "User is not assigned to a branch.");
        }

        return ServiceResult<AccessScope>.SuccessResult(new AccessScope(userBranchId.Value));
    }

    private static string NormalizeRole(string role)
    {
        return role.Replace(" ", string.Empty)
            .Replace("_", string.Empty)
            .Replace("-", string.Empty)
            .Trim()
            .ToLowerInvariant();
    }

    private static bool IsDisposed(string? status)
    {
        return string.Equals(status, DisposedVehicleStatus, StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, LiquidatedVehicleStatus, StringComparison.OrdinalIgnoreCase);
    }

    private sealed record AccessScope(int? RestrictedBranchId);
}
