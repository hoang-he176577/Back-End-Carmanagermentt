using System.Collections.Generic;
using System.Threading.Tasks;
using Models.DTO.Vehicles;
using Models.Models;

namespace Data.Repositories.VehicleAssets.Interfaces;

public interface IVehicleAssetRepository
{
    Task<List<VehicleAssetDto>> GetVehiclesAsync(int? branchId, string? status, bool includeDeleted);
    Task<VehicleAssetDto?> GetVehicleByIdAsync(int id);
    Task<Vehicle?> GetVehicleEntityByIdAsync(int id);
    Task<bool> LicensePlateExistsAsync(string licensePlate);
    Task<bool> LicensePlateExistsForOtherVehicleAsync(string licensePlate, int vehicleId);
    Task<bool> ModelExistsAsync(int modelId);
    Task<bool> BranchExistsAsync(int branchId);
    Task<bool> DriverExistsAsync(int driverId);
    Task<int?> GetUserBranchIdAsync(int userId);
    Task<Vehicle> AddVehicleAsync(Vehicle vehicle);
    Task SaveChangesAsync();
}
