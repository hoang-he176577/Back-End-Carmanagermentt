using Microsoft.EntityFrameworkCore;
using Models.DTO.Accessories;
using Models.Models;
using Service.Services.Common;

namespace Service.Services.Accessories.Implementations;

public sealed partial class AccessoryService
{
    public async Task<ServiceResult<List<VehicleAccessoryRequirementDto>>> GetVehicleAccessoryRequirementsAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        int? modelId)
    {
        var access = await ResolveAccessScopeAsync(actorUserId, roles, requireAnyAccess: true);
        if (!access.Success)
        {
            return ServiceResult<List<VehicleAccessoryRequirementDto>>.Fail(access.StatusCode, access.Message!);
        }

        var query = _context.VehicleAccessoryRequirements.AsNoTracking()
            .Include(x => x.Model)
            .Include(x => x.Accessory)
            .Where(x => x.Accessory.DeletedAt == null);

        if (modelId.HasValue)
        {
            query = query.Where(x => x.ModelId == modelId.Value);
        }

        var items = await query.OrderBy(x => x.Model.ModelName).ThenBy(x => x.Accessory.Name).ToListAsync();
        return ServiceResult<List<VehicleAccessoryRequirementDto>>.SuccessResult(items.Select(MapRequirement).ToList());
    }

    public async Task<ServiceResult<VehicleAccessoryRequirementDto>> CreateVehicleAccessoryRequirementAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        VehicleAccessoryRequirementUpsertRequestDto request)
    {
        var access = await ResolveAccessScopeAsync(actorUserId, roles, requireAnyAccess: true);
        if (!access.Success)
        {
            return ServiceResult<VehicleAccessoryRequirementDto>.Fail(access.StatusCode, access.Message!);
        }

        if (!HasAnyRole(roles, RequirementWriteRoles))
        {
            return ServiceResult<VehicleAccessoryRequirementDto>.Fail(403, "You do not have permission to manage accessory requirements.");
        }

        var validation = await ValidateRequirementRequestAsync(request, null);
        if (!validation.Success)
        {
            return ServiceResult<VehicleAccessoryRequirementDto>.Fail(validation.StatusCode, validation.Message!);
        }

        var entity = new VehicleAccessoryRequirement
        {
            ModelId = request.ModelId,
            AccessoryId = request.AccessoryId,
            RequiredQuantity = request.RequiredQuantity,
            IsMandatory = request.IsMandatory,
            Notes = request.Notes?.Trim()
        };

        _context.VehicleAccessoryRequirements.Add(entity);
        await _context.SaveChangesAsync();

        var result = await GetRequirementByIdAsync(entity.Id);
        return result.Success
            ? ServiceResult<VehicleAccessoryRequirementDto>.SuccessResult(result.Data!, 201)
            : result;
    }

    public async Task<ServiceResult<VehicleAccessoryRequirementDto>> UpdateVehicleAccessoryRequirementAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        int id,
        VehicleAccessoryRequirementUpsertRequestDto request)
    {
        var access = await ResolveAccessScopeAsync(actorUserId, roles, requireAnyAccess: true);
        if (!access.Success)
        {
            return ServiceResult<VehicleAccessoryRequirementDto>.Fail(access.StatusCode, access.Message!);
        }

        if (!HasAnyRole(roles, RequirementWriteRoles))
        {
            return ServiceResult<VehicleAccessoryRequirementDto>.Fail(403, "You do not have permission to manage accessory requirements.");
        }

        var entity = await _context.VehicleAccessoryRequirements.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null)
        {
            return ServiceResult<VehicleAccessoryRequirementDto>.Fail(404, "Vehicle accessory requirement not found.");
        }

        var validation = await ValidateRequirementRequestAsync(request, id);
        if (!validation.Success)
        {
            return ServiceResult<VehicleAccessoryRequirementDto>.Fail(validation.StatusCode, validation.Message!);
        }

        entity.ModelId = request.ModelId;
        entity.AccessoryId = request.AccessoryId;
        entity.RequiredQuantity = request.RequiredQuantity;
        entity.IsMandatory = request.IsMandatory;
        entity.Notes = request.Notes?.Trim();
        await _context.SaveChangesAsync();

        return await GetRequirementByIdAsync(id);
    }

    public async Task<ServiceResult<bool>> DeleteVehicleAccessoryRequirementAsync(int actorUserId, IReadOnlyCollection<string> roles, int id)
    {
        var access = await ResolveAccessScopeAsync(actorUserId, roles, requireAnyAccess: true);
        if (!access.Success)
        {
            return ServiceResult<bool>.Fail(access.StatusCode, access.Message!);
        }

        if (!HasAnyRole(roles, RequirementWriteRoles))
        {
            return ServiceResult<bool>.Fail(403, "You do not have permission to manage accessory requirements.");
        }

        var entity = await _context.VehicleAccessoryRequirements.FirstOrDefaultAsync(x => x.Id == id);
        if (entity == null)
        {
            return ServiceResult<bool>.Fail(404, "Vehicle accessory requirement not found.");
        }

        _context.VehicleAccessoryRequirements.Remove(entity);
        await _context.SaveChangesAsync();
        return ServiceResult<bool>.SuccessResult(true);
    }

    public async Task<ServiceResult<VehicleAccessoryRequirementCheckResultDto>> CheckVehicleAccessoryRequirementsAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        int vehicleId)
    {
        var access = await ResolveAccessScopeAsync(actorUserId, roles, requireAnyAccess: true);
        if (!access.Success)
        {
            return ServiceResult<VehicleAccessoryRequirementCheckResultDto>.Fail(access.StatusCode, access.Message!);
        }

        var vehicle = await _context.Vehicles.AsNoTracking()
            .Include(x => x.Model)
            .FirstOrDefaultAsync(x => x.Id == vehicleId && x.DeletedAt == null);
        if (vehicle == null)
        {
            return ServiceResult<VehicleAccessoryRequirementCheckResultDto>.Fail(404, "Vehicle not found.");
        }

        if (access.Data!.RestrictedBranchId.HasValue && vehicle.CurrentBranchId != access.Data.RestrictedBranchId.Value)
        {
            return ServiceResult<VehicleAccessoryRequirementCheckResultDto>.Fail(403, "You can only access vehicles in your branch.");
        }

        if (!vehicle.ModelId.HasValue)
        {
            return ServiceResult<VehicleAccessoryRequirementCheckResultDto>.Fail(400, "Vehicle does not have a model assigned.");
        }

        var requirements = await _context.VehicleAccessoryRequirements.AsNoTracking()
            .Include(x => x.Accessory)
            .Where(x => x.ModelId == vehicle.ModelId.Value && x.Accessory.DeletedAt == null)
            .ToListAsync();

        var installed = await _context.VehicleAccessories.AsNoTracking()
            .Where(x => x.VehicleId == vehicle.Id && x.DeletedAt == null && x.Status == "Installed")
            .GroupBy(x => x.AccessoryId)
            .Select(x => new { AccessoryId = x.Key, Quantity = x.Sum(y => y.Quantity) })
            .ToDictionaryAsync(x => x.AccessoryId!.Value, x => x.Quantity);

        var result = new VehicleAccessoryRequirementCheckResultDto
        {
            VehicleId = vehicle.Id,
            VehicleLicensePlate = vehicle.LicensePlate,
            ModelId = vehicle.ModelId,
            ModelName = vehicle.Model?.ModelName,
            Items = requirements.Select(requirement =>
            {
                installed.TryGetValue(requirement.AccessoryId, out var installedQuantity);
                var missingQuantity = Math.Max(0, requirement.RequiredQuantity - installedQuantity);
                return new VehicleAccessoryRequirementCheckItemDto
                {
                    AccessoryId = requirement.AccessoryId,
                    AccessoryCode = requirement.Accessory.Code,
                    AccessoryName = requirement.Accessory.Name,
                    ImageUrl = requirement.Accessory.ImageUrl,
                    RequiredQuantity = requirement.RequiredQuantity,
                    InstalledQuantity = installedQuantity,
                    MissingQuantity = missingQuantity,
                    IsMandatory = requirement.IsMandatory,
                    IsSatisfied = missingQuantity == 0,
                    Notes = requirement.Notes
                };
            }).ToList()
        };

        return ServiceResult<VehicleAccessoryRequirementCheckResultDto>.SuccessResult(result);
    }

    public async Task<ServiceResult<IssueVehicleAccessoryResponseDto>> IssueVehicleAccessoryAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        IssueVehicleAccessoryRequestDto request)
    {
        var access = await ResolveAccessScopeAsync(actorUserId, roles, requireAnyAccess: true);
        if (!access.Success)
        {
            return ServiceResult<IssueVehicleAccessoryResponseDto>.Fail(access.StatusCode, access.Message!);
        }

        if (!HasAnyRole(roles, IssueRoles))
        {
            return ServiceResult<IssueVehicleAccessoryResponseDto>.Fail(403, "You do not have permission to issue accessories.");
        }

        if (request == null || request.Quantity <= 0 || request.BranchId <= 0)
        {
            return ServiceResult<IssueVehicleAccessoryResponseDto>.Fail(400, "branchId, accessoryId, vehicleId, and quantity must be greater than 0.");
        }

        var branchIdResult = await ResolveRequestedBranchIdAsync(access.Data!, request.BranchId);
        if (!branchIdResult.Success || !branchIdResult.Data.HasValue)
        {
            return ServiceResult<IssueVehicleAccessoryResponseDto>.Fail(branchIdResult.StatusCode, branchIdResult.Message!);
        }

        var accessory = await _context.Accessories.FirstOrDefaultAsync(x => x.Id == request.AccessoryId && x.DeletedAt == null);
        var vehicle = await _context.Vehicles.FirstOrDefaultAsync(x => x.Id == request.VehicleId && x.DeletedAt == null);
        if (accessory == null || vehicle == null)
        {
            return ServiceResult<IssueVehicleAccessoryResponseDto>.Fail(404, "Accessory or vehicle not found.");
        }

        if (vehicle.CurrentBranchId != branchIdResult.Data.Value)
        {
            return ServiceResult<IssueVehicleAccessoryResponseDto>.Fail(400, "Vehicle does not belong to the selected branch.");
        }

        if (IsDisposed(vehicle.Status))
        {
            return ServiceResult<IssueVehicleAccessoryResponseDto>.Fail(400, "Disposed or liquidated vehicles cannot be assigned accessories.");
        }

        var stock = await _context.BranchAccessoryStocks
            .FirstOrDefaultAsync(x => x.BranchId == branchIdResult.Data.Value && x.AccessoryId == request.AccessoryId);
        if (stock == null || stock.QuantityInStock < request.Quantity)
        {
            return ServiceResult<IssueVehicleAccessoryResponseDto>.Fail(400, "Not enough stock in the selected branch.");
        }

        IssueVehicleAccessoryResponseDto? response = null;
        await ExecuteInTransactionAsync(async () =>
        {
            var now = DateTime.UtcNow;
            stock.QuantityInStock -= request.Quantity;
            stock.UpdatedAt = now;

            var vehicleAccessory = new VehicleAccessory
            {
                VehicleId = request.VehicleId,
                AccessoryId = request.AccessoryId,
                BranchId = branchIdResult.Data.Value,
                Quantity = request.Quantity,
                Status = "Installed",
                InstallDate = request.InstallDate ?? DateOnly.FromDateTime(now),
                Notes = request.Notes?.Trim(),
                InstalledBy = request.InstalledBy ?? actorUserId,
                CreatedAt = now,
                UpdatedAt = now
            };

            _context.VehicleAccessories.Add(vehicleAccessory);
            await _context.SaveChangesAsync();

            var transaction = new AccessoryTransaction
            {
                AccessoryId = request.AccessoryId,
                BranchId = branchIdResult.Data.Value,
                VehicleId = request.VehicleId,
                VehicleAccessoryId = vehicleAccessory.Id,
                TransactionType = "ISSUE",
                ReferenceType = "ISSUE",
                ReferenceId = vehicleAccessory.Id,
                Quantity = request.Quantity,
                UnitPrice = accessory.UnitPrice,
                Notes = request.Notes?.Trim(),
                PerformedBy = request.InstalledBy ?? actorUserId,
                TransactionDate = now
            };

            _context.AccessoryTransactions.Add(transaction);
            await _context.SaveChangesAsync();

            vehicleAccessory.SourceTransactionId = transaction.Id;
            await _context.SaveChangesAsync();

            response = new IssueVehicleAccessoryResponseDto
            {
                VehicleAccessory = await ProjectVehicleAccessoryAsync(vehicleAccessory.Id),
                RemainingStock = stock.QuantityInStock
            };
        });

        return ServiceResult<IssueVehicleAccessoryResponseDto>.SuccessResult(response!, 201);
    }

    public async Task<ServiceResult<VehicleAccessoryDto>> HandleVehicleAccessoryActionAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        int vehicleAccessoryId,
        ReturnVehicleAccessoryRequestDto request)
    {
        var access = await ResolveAccessScopeAsync(actorUserId, roles, requireAnyAccess: true);
        if (!access.Success)
        {
            return ServiceResult<VehicleAccessoryDto>.Fail(access.StatusCode, access.Message!);
        }

        if (!HasAnyRole(roles, IssueRoles))
        {
            return ServiceResult<VehicleAccessoryDto>.Fail(403, "You do not have permission to process vehicle accessories.");
        }

        var action = request.ActionType?.Trim().ToUpperInvariant();
        if (action is not ("RETURN" or "DAMAGED" or "LOST"))
        {
            return ServiceResult<VehicleAccessoryDto>.Fail(400, "ActionType must be RETURN, DAMAGED, or LOST.");
        }

        var entity = await _context.VehicleAccessories
            .Include(x => x.Vehicle)
            .Include(x => x.Accessory)
            .FirstOrDefaultAsync(x => x.Id == vehicleAccessoryId && x.DeletedAt == null);
        if (entity == null || entity.Accessory == null)
        {
            return ServiceResult<VehicleAccessoryDto>.Fail(404, "Vehicle accessory not found.");
        }

        if (entity.Status != "Installed")
        {
            return ServiceResult<VehicleAccessoryDto>.Fail(400, "Only installed records can be processed.");
        }

        if (access.Data!.RestrictedBranchId.HasValue && entity.BranchId != access.Data.RestrictedBranchId.Value)
        {
            return ServiceResult<VehicleAccessoryDto>.Fail(403, "You can only process accessories in your branch.");
        }

        await ExecuteInTransactionAsync(async () =>
        {
            var now = DateTime.UtcNow;
            entity.Status = action switch
            {
                "RETURN" => "Returned",
                "DAMAGED" => "Damaged",
                "LOST" => "Lost",
                _ => entity.Status
            };
            entity.RemoveDate = request.RemoveDate ?? DateOnly.FromDateTime(now);
            entity.RemovedBy = request.RemovedBy ?? actorUserId;
            entity.Notes = request.Notes?.Trim();
            entity.UpdatedAt = now;

            if (action == "RETURN")
            {
                var stock = await _context.BranchAccessoryStocks
                    .FirstOrDefaultAsync(x => x.BranchId == entity.BranchId && x.AccessoryId == entity.AccessoryId);
                if (stock == null)
                {
                    stock = new BranchAccessoryStock
                    {
                        BranchId = entity.BranchId!.Value,
                        AccessoryId = entity.AccessoryId!.Value,
                        QuantityInStock = 0,
                        MinimumStock = null,
                        CreatedAt = now,
                        UpdatedAt = now
                    };
                    _context.BranchAccessoryStocks.Add(stock);
                }

                stock.QuantityInStock += entity.Quantity;
                stock.UpdatedAt = now;
            }

            _context.AccessoryTransactions.Add(new AccessoryTransaction
            {
                AccessoryId = entity.AccessoryId!.Value,
                BranchId = entity.BranchId,
                VehicleId = entity.VehicleId,
                VehicleAccessoryId = entity.Id,
                TransactionType = action,
                ReferenceType = action,
                ReferenceId = entity.Id,
                Quantity = entity.Quantity,
                UnitPrice = entity.Accessory.UnitPrice,
                Notes = request.Notes?.Trim(),
                PerformedBy = request.RemovedBy ?? actorUserId,
                TransactionDate = now
            });

            await _context.SaveChangesAsync();
        });

        return ServiceResult<VehicleAccessoryDto>.SuccessResult(await ProjectVehicleAccessoryAsync(entity.Id));
    }

    public async Task<ServiceResult<List<VehicleAccessoryDto>>> GetVehicleAccessoriesAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        int vehicleId,
        bool activeOnly)
    {
        var access = await ResolveAccessScopeAsync(actorUserId, roles, requireAnyAccess: true);
        if (!access.Success)
        {
            return ServiceResult<List<VehicleAccessoryDto>>.Fail(access.StatusCode, access.Message!);
        }

        var vehicle = await _context.Vehicles.AsNoTracking().FirstOrDefaultAsync(x => x.Id == vehicleId && x.DeletedAt == null);
        if (vehicle == null)
        {
            return ServiceResult<List<VehicleAccessoryDto>>.Fail(404, "Vehicle not found.");
        }

        if (access.Data!.RestrictedBranchId.HasValue && vehicle.CurrentBranchId != access.Data.RestrictedBranchId.Value)
        {
            return ServiceResult<List<VehicleAccessoryDto>>.Fail(403, "You can only access vehicles in your branch.");
        }

        var query = _context.VehicleAccessories.AsNoTracking()
            .Include(x => x.Accessory)
            .Include(x => x.Vehicle)
            .Include(x => x.Branch)
            .Include(x => x.InstalledByNavigation)
            .Include(x => x.RemovedByNavigation)
            .Where(x => x.VehicleId == vehicleId && x.DeletedAt == null);

        if (activeOnly)
        {
            query = query.Where(x => x.Status == "Installed");
        }

        var items = await query.OrderByDescending(x => x.CreatedAt).ToListAsync();
        return ServiceResult<List<VehicleAccessoryDto>>.SuccessResult(items.Select(MapVehicleAccessory).ToList());
    }

    public async Task<ServiceResult<List<AccessoryTransactionDto>>> GetTransactionsAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        int? branchId,
        int? accessoryId,
        int? vehicleId,
        string? transactionType,
        string? referenceType,
        DateTime? fromDate,
        DateTime? toDate,
        int? page,
        int? pageSize)
    {
        var access = await ResolveAccessScopeAsync(actorUserId, roles, requireAnyAccess: true);
        if (!access.Success)
        {
            return ServiceResult<List<AccessoryTransactionDto>>.Fail(access.StatusCode, access.Message!);
        }

        if (!string.IsNullOrWhiteSpace(transactionType) && !AllowedTransactionTypes.Contains(transactionType.Trim()))
        {
            return ServiceResult<List<AccessoryTransactionDto>>.Fail(400, "Invalid transaction type.");
        }

        if (!string.IsNullOrWhiteSpace(referenceType) && !AllowedReferenceTypes.Contains(referenceType.Trim()))
        {
            return ServiceResult<List<AccessoryTransactionDto>>.Fail(400, "Invalid reference type.");
        }

        if (fromDate.HasValue && toDate.HasValue && fromDate > toDate)
        {
            return ServiceResult<List<AccessoryTransactionDto>>.Fail(400, "fromDate must be less than or equal to toDate.");
        }

        var scopedBranchId = await ResolveRequestedBranchIdAsync(access.Data!, branchId);
        if (!scopedBranchId.Success)
        {
            return ServiceResult<List<AccessoryTransactionDto>>.Fail(scopedBranchId.StatusCode, scopedBranchId.Message!);
        }

        var query = _context.AccessoryTransactions.AsNoTracking()
            .Include(x => x.Accessory)
            .Include(x => x.Branch)
            .Include(x => x.Vehicle)
            .Include(x => x.PerformedByNavigation)
            .Where(x => x.Accessory.DeletedAt == null);

        if (scopedBranchId.Data.HasValue)
        {
            query = query.Where(x => x.BranchId == scopedBranchId.Data.Value);
        }

        if (accessoryId.HasValue)
        {
            query = query.Where(x => x.AccessoryId == accessoryId.Value);
        }

        if (vehicleId.HasValue)
        {
            query = query.Where(x => x.VehicleId == vehicleId.Value);
        }

        if (!string.IsNullOrWhiteSpace(transactionType))
        {
            query = query.Where(x => x.TransactionType == NormalizeTransactionType(transactionType));
        }

        if (!string.IsNullOrWhiteSpace(referenceType))
        {
            query = query.Where(x => x.ReferenceType == NormalizeReferenceType(referenceType));
        }

        if (fromDate.HasValue)
        {
            query = query.Where(x => x.TransactionDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(x => x.TransactionDate <= toDate.Value);
        }

        query = query.OrderByDescending(x => x.TransactionDate).ThenByDescending(x => x.Id);

        if (page.HasValue && pageSize.HasValue && page > 0 && pageSize > 0)
        {
            query = query.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value);
        }

        var items = await query
            .Select(x => new AccessoryTransactionDto
            {
                Id = x.Id,
                AccessoryId = x.AccessoryId,
                AccessoryCode = x.Accessory.Code,
                AccessoryName = x.Accessory.Name,
                BranchId = x.BranchId,
                BranchName = x.Branch != null ? x.Branch.Name : null,
                VehicleId = x.VehicleId,
                VehicleLicensePlate = x.Vehicle != null ? x.Vehicle.LicensePlate : null,
                VehicleAccessoryId = x.VehicleAccessoryId,
                TransactionType = x.TransactionType,
                ReferenceType = x.ReferenceType,
                ReferenceId = x.ReferenceId,
                Quantity = x.Quantity,
                TransactionDate = x.TransactionDate,
                UnitPrice = x.UnitPrice,
                Notes = x.Notes,
                PerformedBy = x.PerformedBy,
                PerformedByName = x.PerformedByNavigation != null ? x.PerformedByNavigation.Name : null
            })
            .ToListAsync();

        return ServiceResult<List<AccessoryTransactionDto>>.SuccessResult(items);
    }
}
