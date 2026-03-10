using Microsoft.EntityFrameworkCore.Storage;
using Models.DTO.Accessories;
using Models.Models;

namespace Data.Repositories.Accessories.Interfaces;

public interface IAccessoryRepository
{
    Task<List<AccessoryDto>> GetAccessoriesAsync(string? keyword, string? type, bool? isActive, int? page, int? pageSize);
    Task<AccessoryDto?> GetAccessoryByIdAsync(int id);
    Task<Accessory?> GetAccessoryEntityByIdAsync(int id);
    Task<bool> AccessoryCodeExistsAsync(string code, int? excludeId = null);
    Task<Accessory> AddAccessoryAsync(Accessory accessory);

    Task<Vehicle?> GetVehicleEntityByIdAsync(int id);
    Task<int?> GetVehicleBranchIdAsync(int vehicleId);
    Task<int?> GetUserBranchIdAsync(int userId);
    Task<VehicleAccessory?> GetVehicleAccessoryEntityByIdAsync(int id);
    Task<VehicleAccessory?> GetVehicleAccessoryWithAccessoryAsync(int id);
    Task<VehicleAccessory> AddVehicleAccessoryAsync(VehicleAccessory vehicleAccessory);
    Task<List<VehicleAccessoryDto>> GetVehicleAccessoriesByVehicleIdAsync(int vehicleId, bool activeOnly);

    Task AddAccessoryTransactionAsync(AccessoryTransaction transaction);
    Task<List<AccessoryTransactionDto>> GetAccessoryTransactionsAsync(
        int? accessoryId,
        int? vehicleId,
        string? transactionType,
        DateTime? fromDate,
        DateTime? toDate,
        int? branchId,
        int? page,
        int? pageSize);

    Task<IDbContextTransaction> BeginTransactionAsync();
    Task SaveChangesAsync();
}
