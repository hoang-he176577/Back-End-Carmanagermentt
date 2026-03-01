using System.Collections.Generic;
using System.Threading.Tasks;
using Models.DTO.Vehicles;
using Models.Models;

namespace Data.Repositories.Interfaces;

public interface IVehicleAssetRepository
{
    Task<List<VehicleAssetDto>> GetVehiclesAsync(int? branchId, string? status, bool includeDeleted);
    Task<VehicleAssetDto?> GetVehicleByIdAsync(int id);

    Task<bool> LicensePlateExistsAsync(string licensePlate);
    Task<bool> VinExistsAsync(string vin);
    Task<bool> EngineNumberExistsAsync(string engineNumber);
    Task<bool> ChassisNumberExistsAsync(string chassisNumber);

    Task<bool> ModelExistsAsync(int modelId);
    Task<bool> BranchExistsAsync(int branchId);
    Task<bool> DriverExistsAsync(int driverId);

    Task<Vehicle> AddVehicleAsync(Vehicle vehicle);

    Task AddRegistrationRecordAsync(RegistrationRecord record);
    Task AddInsuranceRecordAsync(InsuranceRecord record);
    Task AddAssetChangeLogAsync(AssetChangeLog log);

    Task<Vehicle?> GetVehicleEntityByIdAsync(int id);
    Task<Driver?> GetDriverByIdAsync(int id);
    Task<VehicleDriverHistory?> GetLatestActiveDriverHistoryAsync(int vehicleId);
    Task AddDriverHistoryAsync(VehicleDriverHistory history);

    Task SaveChangesAsync();
}
