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
    Task<bool> VinExistsAsync(string vin);
    Task<bool> EngineNumberExistsAsync(string engineNumber);
    Task<bool> ChassisNumberExistsAsync(string chassisNumber);

    Task<bool> ModelExistsAsync(int modelId);
    Task<bool> BranchExistsAsync(int branchId);
    Task<bool> DriverExistsAsync(int driverId);
    Task<int?> GetUserBranchIdAsync(int userId);

    Task<Vehicle> AddVehicleAsync(Vehicle vehicle);

    Task AddRegistrationRecordAsync(RegistrationRecord record);
    Task AddInsuranceRecordAsync(InsuranceRecord record);
    Task AddAssetChangeLogAsync(AssetChangeLog log);

    Task<Driver?> GetDriverByIdAsync(int id);
    Task<VehicleDriverHistory?> GetLatestActiveDriverHistoryAsync(int vehicleId);
    Task AddDriverHistoryAsync(VehicleDriverHistory history);
    Task SaveChangesAsync();

    // Dropdown data
    Task<List<VehicleModel>> GetAllModelsAsync();
    Task<List<Driver>> GetAllDriversAsync();
    Task<List<Branch>> GetAllBranchesAsync();

    // Assign operations
    Task<Driver?> GetDriverByIdAsync(int driverId);
}
