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
    Task<bool> ModelExistsAsync(int modelId);
    Task<bool> BranchExistsAsync(int branchId);
    Task<bool> DriverExistsAsync(int driverId);
    Task<Vehicle> AddVehicleAsync(Vehicle vehicle);
}
