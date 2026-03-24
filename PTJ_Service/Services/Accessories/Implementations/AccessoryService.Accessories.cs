using Microsoft.EntityFrameworkCore;
using Models.DTO.Accessories;
using Models.Models;
using Service.Services.Common;

namespace Service.Services.Accessories.Implementations;

public sealed partial class AccessoryService
{
    public async Task<ServiceResult<List<AccessoryDto>>> GetAccessoriesAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        string? keyword,
        string? type,
        bool? isActive,
        int? page,
        int? pageSize)
    {
        var access = await ResolveAccessScopeAsync(actorUserId, roles, requireAnyAccess: true);
        if (!access.Success)
        {
            return ServiceResult<List<AccessoryDto>>.Fail(access.StatusCode, access.Message!);
        }

        if (!string.IsNullOrWhiteSpace(type) && !AllowedAccessoryTypes.Contains(type.Trim()))
        {
            return ServiceResult<List<AccessoryDto>>.Fail(400, "Invalid accessory type.");
        }

        var scopedBranchId = access.Data!.RestrictedBranchId;
        var query = _context.Accessories.AsNoTracking().Where(x => x.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var key = keyword.Trim();
            query = query.Where(x =>
                (x.Code != null && x.Code.Contains(key)) ||
                (x.Name != null && x.Name.Contains(key)));
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            var normalizedType = NormalizeAccessoryType(type);
            query = query.Where(x => x.Type == normalizedType);
        }

        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        query = query.OrderBy(x => x.Name).ThenBy(x => x.Id);

        if (page.HasValue && pageSize.HasValue && page > 0 && pageSize > 0)
        {
            query = query.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value);
        }

        var items = await query
            .Select(x => new AccessoryDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Type = x.Type,
                QuantityInStock = scopedBranchId.HasValue
                    ? x.BranchAccessoryStocks
                        .Where(bs => bs.BranchId == scopedBranchId.Value)
                        .Select(bs => (int?)bs.QuantityInStock)
                        .FirstOrDefault() ?? 0
                    : x.BranchAccessoryStocks.Sum(bs => (int?)bs.QuantityInStock) ?? 0,
                UnitPrice = x.UnitPrice,
                MinimumStock = scopedBranchId.HasValue
                    ? x.BranchAccessoryStocks
                        .Where(bs => bs.BranchId == scopedBranchId.Value)
                        .Select(bs => bs.MinimumStock)
                        .FirstOrDefault()
                    : x.MinimumStock,
                IsActive = x.IsActive,
                ImageUrl = x.ImageUrl,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();

        return ServiceResult<List<AccessoryDto>>.SuccessResult(items);
    }

    public async Task<ServiceResult<AccessoryDto>> GetAccessoryByIdAsync(int actorUserId, IReadOnlyCollection<string> roles, int id)
    {
        var access = await ResolveAccessScopeAsync(actorUserId, roles, requireAnyAccess: true);
        if (!access.Success)
        {
            return ServiceResult<AccessoryDto>.Fail(access.StatusCode, access.Message!);
        }

        var scopedBranchId = access.Data!.RestrictedBranchId;
        var item = await _context.Accessories.AsNoTracking()
            .Where(x => x.Id == id && x.DeletedAt == null)
            .Select(x => new AccessoryDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Type = x.Type,
                QuantityInStock = scopedBranchId.HasValue
                    ? x.BranchAccessoryStocks
                        .Where(bs => bs.BranchId == scopedBranchId.Value)
                        .Select(bs => (int?)bs.QuantityInStock)
                        .FirstOrDefault() ?? 0
                    : x.BranchAccessoryStocks.Sum(bs => (int?)bs.QuantityInStock) ?? 0,
                UnitPrice = x.UnitPrice,
                MinimumStock = scopedBranchId.HasValue
                    ? x.BranchAccessoryStocks
                        .Where(bs => bs.BranchId == scopedBranchId.Value)
                        .Select(bs => bs.MinimumStock)
                        .FirstOrDefault()
                    : x.MinimumStock,
                IsActive = x.IsActive,
                ImageUrl = x.ImageUrl,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync();

        return item == null
            ? ServiceResult<AccessoryDto>.Fail(404, "Accessory not found.")
            : ServiceResult<AccessoryDto>.SuccessResult(item);
    }

    public async Task<ServiceResult<AccessoryDto>> CreateAccessoryAsync(int actorUserId, IReadOnlyCollection<string> roles, AccessoryCreateRequestDto request)
    {
        var access = await ResolveAccessScopeAsync(actorUserId, roles, requireAnyAccess: true);
        if (!access.Success)
        {
            return ServiceResult<AccessoryDto>.Fail(access.StatusCode, access.Message!);
        }

        if (!HasAnyRole(roles, AccessoryWriteRoles))
        {
            return ServiceResult<AccessoryDto>.Fail(403, "You do not have permission to create accessories.");
        }

        if (request == null)
        {
            return ServiceResult<AccessoryDto>.Fail(400, "Request body is required.");
        }

        if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name))
        {
            return ServiceResult<AccessoryDto>.Fail(400, "Code and name are required.");
        }

        if (!AllowedAccessoryTypes.Contains(request.Type))
        {
            return ServiceResult<AccessoryDto>.Fail(400, "Type must be Reusable, Consumable, or Fixed.");
        }

        var normalizedCode = request.Code.Trim();
        var exists = await _context.Accessories.AnyAsync(x => x.Code == normalizedCode && x.DeletedAt == null);
        if (exists)
        {
            return ServiceResult<AccessoryDto>.Fail(409, "Accessory code already exists.");
        }

        var now = DateTime.UtcNow;
        var entity = new Accessory
        {
            Code = normalizedCode,
            Name = request.Name.Trim(),
            Type = NormalizeAccessoryType(request.Type),
            QuantityInStock = 0,
            UnitPrice = request.UnitPrice,
            MinimumStock = request.MinimumStock,
            IsActive = request.IsActive,
            ImageUrl = request.ImageUrl?.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };

        _context.Accessories.Add(entity);
        await _context.SaveChangesAsync();

        var result = await GetAccessoryByIdAsync(actorUserId, roles, entity.Id);
        return result.Success
            ? ServiceResult<AccessoryDto>.SuccessResult(result.Data!, 201)
            : result;
    }

    public async Task<ServiceResult<AccessoryDto>> UpdateAccessoryAsync(int actorUserId, IReadOnlyCollection<string> roles, int id, AccessoryUpdateRequestDto request)
    {
        var access = await ResolveAccessScopeAsync(actorUserId, roles, requireAnyAccess: true);
        if (!access.Success)
        {
            return ServiceResult<AccessoryDto>.Fail(access.StatusCode, access.Message!);
        }

        if (!HasAnyRole(roles, AccessoryWriteRoles))
        {
            return ServiceResult<AccessoryDto>.Fail(403, "You do not have permission to update accessories.");
        }

        var entity = await _context.Accessories.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null);
        if (entity == null)
        {
            return ServiceResult<AccessoryDto>.Fail(404, "Accessory not found.");
        }

        if (!string.IsNullOrWhiteSpace(request.Code))
        {
            var normalizedCode = request.Code.Trim();
            var codeExists = await _context.Accessories.AnyAsync(x => x.Id != id && x.Code == normalizedCode && x.DeletedAt == null);
            if (codeExists)
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

        if (request.UnitPrice.HasValue && request.UnitPrice.Value < 0)
        {
            return ServiceResult<AccessoryDto>.Fail(400, "UnitPrice must be >= 0.");
        }

        if (request.MinimumStock.HasValue && request.MinimumStock.Value < 0)
        {
            return ServiceResult<AccessoryDto>.Fail(400, "MinimumStock must be >= 0.");
        }

        if (request.UnitPrice.HasValue)
        {
            entity.UnitPrice = request.UnitPrice.Value;
        }

        if (request.MinimumStock.HasValue)
        {
            entity.MinimumStock = request.MinimumStock.Value;
        }

        if (request.IsActive.HasValue)
        {
            entity.IsActive = request.IsActive.Value;
        }

        if (request.ImageUrl != null)
        {
            entity.ImageUrl = request.ImageUrl.Trim();
        }

        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GetAccessoryByIdAsync(actorUserId, roles, entity.Id);
    }

    public Task<ServiceResult<AccessoryDto>> ImportAccessoryAsync(int actorUserId, IReadOnlyCollection<string> roles, int id, AccessoryImportRequestDto request)
    {
        return Task.FromResult(ServiceResult<AccessoryDto>.Fail(400, "Direct import is disabled. Use accessory goods receipt workflow instead."));
    }

    public async Task<ServiceResult<List<BranchAccessoryStockDto>>> GetBranchStocksAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        int? branchId,
        int? accessoryId,
        bool belowMinimumOnly,
        int? page,
        int? pageSize)
    {
        var access = await ResolveAccessScopeAsync(actorUserId, roles, requireAnyAccess: true);
        if (!access.Success)
        {
            return ServiceResult<List<BranchAccessoryStockDto>>.Fail(access.StatusCode, access.Message!);
        }

        var scopedBranchId = await ResolveRequestedBranchIdAsync(access.Data!, branchId);
        if (!scopedBranchId.Success)
        {
            return ServiceResult<List<BranchAccessoryStockDto>>.Fail(scopedBranchId.StatusCode, scopedBranchId.Message!);
        }

        var query = _context.BranchAccessoryStocks.AsNoTracking()
            .Where(x => x.Accessory.DeletedAt == null && x.Branch.DeletedAt == null);

        if (scopedBranchId.Data.HasValue)
        {
            query = query.Where(x => x.BranchId == scopedBranchId.Data.Value);
        }

        if (accessoryId.HasValue)
        {
            query = query.Where(x => x.AccessoryId == accessoryId.Value);
        }

        if (belowMinimumOnly)
        {
            query = query.Where(x => x.MinimumStock.HasValue && x.QuantityInStock < x.MinimumStock.Value);
        }

        query = query.OrderBy(x => x.Branch.Name).ThenBy(x => x.Accessory.Name);

        if (page.HasValue && pageSize.HasValue && page > 0 && pageSize > 0)
        {
            query = query.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value);
        }

        var items = await query
            .Select(x => new BranchAccessoryStockDto
            {
                Id = x.Id,
                BranchId = x.BranchId,
                BranchName = x.Branch.Name,
                AccessoryId = x.AccessoryId,
                AccessoryCode = x.Accessory.Code,
                AccessoryName = x.Accessory.Name,
                AccessoryType = x.Accessory.Type,
                ImageUrl = x.Accessory.ImageUrl,
                QuantityInStock = x.QuantityInStock,
                MinimumStock = x.MinimumStock,
                IsBelowMinimum = x.MinimumStock.HasValue && x.QuantityInStock < x.MinimumStock.Value,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();

        return ServiceResult<List<BranchAccessoryStockDto>>.SuccessResult(items);
    }

    public async Task<ServiceResult<BranchAccessoryStockDto>> UpsertBranchStockAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        BranchAccessoryStockUpsertRequestDto request)
    {
        var access = await ResolveAccessScopeAsync(actorUserId, roles, requireAnyAccess: true);
        if (!access.Success)
        {
            return ServiceResult<BranchAccessoryStockDto>.Fail(access.StatusCode, access.Message!);
        }

        if (!HasAnyRole(roles, BranchStockWriteRoles))
        {
            return ServiceResult<BranchAccessoryStockDto>.Fail(403, "You do not have permission to update branch stock.");
        }

        var branchIdResult = await ResolveRequestedBranchIdAsync(access.Data!, request.BranchId);
        if (!branchIdResult.Success || !branchIdResult.Data.HasValue)
        {
            return ServiceResult<BranchAccessoryStockDto>.Fail(branchIdResult.StatusCode, branchIdResult.Message!);
        }

        if (request.QuantityInStock < 0 || (request.MinimumStock.HasValue && request.MinimumStock.Value < 0))
        {
            return ServiceResult<BranchAccessoryStockDto>.Fail(400, "Stock values must be >= 0.");
        }

        var branchExists = await _context.Branches.AnyAsync(x => x.Id == branchIdResult.Data.Value && x.DeletedAt == null);
        var accessory = await _context.Accessories.FirstOrDefaultAsync(x => x.Id == request.AccessoryId && x.DeletedAt == null);
        if (!branchExists || accessory == null)
        {
            return ServiceResult<BranchAccessoryStockDto>.Fail(404, "Branch or accessory not found.");
        }

        BranchAccessoryStock? stock = null;
        await ExecuteInTransactionAsync(async () =>
        {
            var now = DateTime.UtcNow;
            stock = await _context.BranchAccessoryStocks
                .FirstOrDefaultAsync(x => x.BranchId == branchIdResult.Data.Value && x.AccessoryId == request.AccessoryId);

            var previousQuantity = stock?.QuantityInStock ?? 0;
            if (stock == null)
            {
                stock = new BranchAccessoryStock
                {
                    BranchId = branchIdResult.Data.Value,
                    AccessoryId = request.AccessoryId,
                    QuantityInStock = request.QuantityInStock,
                    MinimumStock = request.MinimumStock,
                    CreatedAt = now,
                    UpdatedAt = now
                };
                _context.BranchAccessoryStocks.Add(stock);
            }
            else
            {
                stock.QuantityInStock = request.QuantityInStock;
                stock.MinimumStock = request.MinimumStock;
                stock.UpdatedAt = now;
            }

            await _context.SaveChangesAsync();

            var delta = request.QuantityInStock - previousQuantity;
            if (delta != 0)
            {
                _context.AccessoryTransactions.Add(new AccessoryTransaction
                {
                    AccessoryId = request.AccessoryId,
                    BranchId = branchIdResult.Data.Value,
                    TransactionType = "ADJUST",
                    ReferenceType = "ADJUST",
                    ReferenceId = stock.Id,
                    Quantity = delta,
                    UnitPrice = accessory.UnitPrice,
                    Notes = "Manual stock adjustment.",
                    PerformedBy = actorUserId,
                    TransactionDate = now
                });
                await _context.SaveChangesAsync();
            }
        });

        return await GetBranchStockByIdAsync(stock!.Id);
    }

    private async Task<ServiceResult<BranchAccessoryStockDto>> GetBranchStockByIdAsync(int id)
    {
        var item = await _context.BranchAccessoryStocks.AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new BranchAccessoryStockDto
            {
                Id = x.Id,
                BranchId = x.BranchId,
                BranchName = x.Branch.Name,
                AccessoryId = x.AccessoryId,
                AccessoryCode = x.Accessory.Code,
                AccessoryName = x.Accessory.Name,
                AccessoryType = x.Accessory.Type,
                ImageUrl = x.Accessory.ImageUrl,
                QuantityInStock = x.QuantityInStock,
                MinimumStock = x.MinimumStock,
                IsBelowMinimum = x.MinimumStock.HasValue && x.QuantityInStock < x.MinimumStock.Value,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync();

        return item == null
            ? ServiceResult<BranchAccessoryStockDto>.Fail(404, "Branch stock not found.")
            : ServiceResult<BranchAccessoryStockDto>.SuccessResult(item);
    }
}
