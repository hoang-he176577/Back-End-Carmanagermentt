using System.Collections.Generic;
using System.Threading.Tasks;
using Models.DTO.Vehicles;
using Service.Services.Common;

namespace Service.Services.VehicleAssets.Interfaces;

public interface IVehicleAssetService
{
    Task<ServiceResult<List<VehicleAssetDto>>> GetVehiclesAsync(int actorUserId, IReadOnlyCollection<string> roles, int? branchId, string? status, bool includeDeleted);
    Task<ServiceResult<VehicleAssetDto>> GetVehicleByIdAsync(int actorUserId, IReadOnlyCollection<string> roles, int id);
    Task<ServiceResult<VehicleAssetDto>> CreateVehicleAsync(int actorUserId, IReadOnlyCollection<string> roles, VehicleCreateRequestDto request);
    Task<ServiceResult<VehicleAssetDto>> UpdateVehicleAsync(int actorUserId, IReadOnlyCollection<string> roles, int id, VehicleUpdateRequestDto request);

    // Dropdown data methods
    Task<List<VehicleModelDto>> GetVehicleModelsAsync();
    Task<List<BranchDto>> GetBranchesAsync();
    Task<List<DriverDto>> GetDriversAsync();
}
