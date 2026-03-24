using Data.Repositories.Accessories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models.DTO.Accessories;
using Models.Models;

namespace Data.Repositories.Accessories.Implementations;

public sealed class AccessoryRepository : IAccessoryRepository
{
    private readonly CarManagerContext _context;

    public AccessoryRepository(CarManagerContext context)
    {
        _context = context;
    }

    public async Task<List<AccessoryDto>> GetAccessoriesAsync(string? keyword, string? type, bool? isActive, int? page, int? pageSize)
    {
        var query = _context.Accessories.AsNoTracking()
            .Where(x => x.DeletedAt == null)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            var key = keyword.Trim();
            query = query.Where(x =>
                (x.Code != null && x.Code.Contains(key)) ||
                (x.Name != null && x.Name.Contains(key)));
        }

        if (!string.IsNullOrWhiteSpace(type))
        {
            var normalizedType = type.Trim();
            query = query.Where(x => x.Type == normalizedType);
        }

        if (isActive.HasValue)
        {
            query = query.Where(x => x.IsActive == isActive.Value);
        }

        query = query.OrderByDescending(x => x.CreatedAt).ThenByDescending(x => x.Id);

        if (page.HasValue && pageSize.HasValue && page.Value > 0 && pageSize.Value > 0)
        {
            var skip = (page.Value - 1) * pageSize.Value;
            query = query.Skip(skip).Take(pageSize.Value);
        }

        return await query.Select(x => new AccessoryDto
        {
            Id = x.Id,
            Code = x.Code,
            Name = x.Name,
            Type = x.Type,
            QuantityInStock = x.BranchAccessoryStocks.Sum(bs => (int?)bs.QuantityInStock) ?? 0,
            UnitPrice = x.UnitPrice,
            MinimumStock = x.MinimumStock,
            IsActive = x.IsActive,
            ImageUrl = x.ImageUrl,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt
        }).ToListAsync();
    }

    public async Task<AccessoryDto?> GetAccessoryByIdAsync(int id)
    {
        return await _context.Accessories.AsNoTracking()
            .Where(x => x.Id == id && x.DeletedAt == null)
            .Select(x => new AccessoryDto
            {
                Id = x.Id,
                Code = x.Code,
                Name = x.Name,
                Type = x.Type,
                QuantityInStock = x.BranchAccessoryStocks.Sum(bs => (int?)bs.QuantityInStock) ?? 0,
                UnitPrice = x.UnitPrice,
                MinimumStock = x.MinimumStock,
                IsActive = x.IsActive,
                ImageUrl = x.ImageUrl,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .FirstOrDefaultAsync();
    }

    public Task<Accessory?> GetAccessoryEntityByIdAsync(int id)
    {
        return _context.Accessories.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null);
    }

    public Task<bool> AccessoryCodeExistsAsync(string code, int? excludeId = null)
    {
        var query = _context.Accessories.AsNoTracking().Where(x => x.Code == code && x.DeletedAt == null);
        if (excludeId.HasValue)
        {
            query = query.Where(x => x.Id != excludeId.Value);
        }

        return query.AnyAsync();
    }

    public async Task<Accessory> AddAccessoryAsync(Accessory accessory)
    {
        _context.Accessories.Add(accessory);
        await _context.SaveChangesAsync();
        return accessory;
    }

    public Task<Vehicle?> GetVehicleEntityByIdAsync(int id)
    {
        return _context.Vehicles.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null);
    }

    public Task<int?> GetVehicleBranchIdAsync(int vehicleId)
    {
        return _context.Vehicles.AsNoTracking()
            .Where(x => x.Id == vehicleId && x.DeletedAt == null)
            .Select(x => x.CurrentBranchId)
            .FirstOrDefaultAsync();
    }

    public Task<int?> GetUserBranchIdAsync(int userId)
    {
        return _context.Users.AsNoTracking()
            .Where(x => x.Id == userId && x.DeletedAt == null)
            .Select(x => x.BranchId)
            .FirstOrDefaultAsync();
    }

    public Task<VehicleAccessory?> GetVehicleAccessoryEntityByIdAsync(int id)
    {
        return _context.VehicleAccessories.FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null);
    }

    public Task<VehicleAccessory?> GetVehicleAccessoryWithAccessoryAsync(int id)
    {
        return _context.VehicleAccessories
            .Include(x => x.Accessory)
            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null);
    }

    public async Task<VehicleAccessory> AddVehicleAccessoryAsync(VehicleAccessory vehicleAccessory)
    {
        _context.VehicleAccessories.Add(vehicleAccessory);
        await _context.SaveChangesAsync();
        return vehicleAccessory;
    }

    public Task<List<VehicleAccessoryDto>> GetVehicleAccessoriesByVehicleIdAsync(int vehicleId, bool activeOnly)
    {
        var query = _context.VehicleAccessories.AsNoTracking()
            .Include(x => x.Accessory)
            .Where(x => x.VehicleId == vehicleId && x.DeletedAt == null);

        if (activeOnly)
        {
            query = query.Where(x => x.Status == "Installed");
        }

        return query
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new VehicleAccessoryDto
            {
                Id = x.Id,
                VehicleId = x.VehicleId,
                AccessoryId = x.AccessoryId,
                AccessoryCode = x.Accessory != null ? x.Accessory.Code : null,
                AccessoryName = x.Accessory != null ? x.Accessory.Name : null,
                AccessoryType = x.Accessory != null ? x.Accessory.Type : null,
                Quantity = x.Quantity,
                Status = x.Status,
                InstallDate = x.InstallDate,
                RemoveDate = x.RemoveDate,
                Notes = x.Notes,
                InstalledBy = x.InstalledBy,
                RemovedBy = x.RemovedBy,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            })
            .ToListAsync();
    }

    public async Task AddAccessoryTransactionAsync(AccessoryTransaction transaction)
    {
        _context.AccessoryTransactions.Add(transaction);
        await _context.SaveChangesAsync();
    }

    public Task<List<AccessoryTransactionDto>> GetAccessoryTransactionsAsync(
        int? accessoryId,
        int? vehicleId,
        string? transactionType,
        DateTime? fromDate,
        DateTime? toDate,
        int? branchId,
        int? page,
        int? pageSize)
    {
        var query = _context.AccessoryTransactions.AsNoTracking().AsQueryable();

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
            var normalizedType = transactionType.Trim();
            query = query.Where(x => x.TransactionType == normalizedType);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(x => x.TransactionDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(x => x.TransactionDate <= toDate.Value);
        }

        if (branchId.HasValue)
        {
            var scopedBranchId = branchId.Value;
            query = query.Where(x => x.BranchId == scopedBranchId);
        }

        query = query.OrderByDescending(x => x.TransactionDate).ThenByDescending(x => x.Id);

        if (page.HasValue && pageSize.HasValue && page.Value > 0 && pageSize.Value > 0)
        {
            var skip = (page.Value - 1) * pageSize.Value;
            query = query.Skip(skip).Take(pageSize.Value);
        }

        return query.Select(x => new AccessoryTransactionDto
        {
            Id = x.Id,
            AccessoryId = x.AccessoryId,
            AccessoryCode = x.Accessory != null ? x.Accessory.Code : null,
            AccessoryName = x.Accessory != null ? x.Accessory.Name : null,
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
        }).ToListAsync();
    }

    public Task ExecuteInTransactionAsync(Func<Task> operation)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        return strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                await operation();
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        });
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}
