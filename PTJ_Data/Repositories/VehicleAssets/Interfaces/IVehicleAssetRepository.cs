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

    // Dropdown data
    Task<List<VehicleModel>> GetAllModelsAsync();
    Task<List<Driver>> GetAllDriversAsync();
    Task<List<Branch>> GetAllBranchesAsync();

    Task<Driver?> GetDriverByIdAsync(int id);
    Task<VehicleDriverHistory?> GetLatestActiveDriverHistoryAsync(int vehicleId);
    Task AddDriverHistoryAsync(VehicleDriverHistory history);
    Task SaveChangesAsync();

    // Dropdown data methods
    Task<List<VehicleModelDto>> GetVehicleModelsAsync();
    Task<List<BranchDto>> GetBranchesAsync();
    Task<List<DriverDto>> GetDriversAsync();
}
