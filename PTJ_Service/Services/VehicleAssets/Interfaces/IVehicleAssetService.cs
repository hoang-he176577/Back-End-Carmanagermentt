using System.Collections.Generic;
using System.Threading.Tasks;
using Models.DTO.Vehicles;
using Models.Models;
using Service.Services.Common;

namespace Service.Services.VehicleAssets.Interfaces;

public interface IVehicleAssetService
{
    Task<ServiceResult<List<VehicleAssetDto>>> GetVehiclesAsync(int actorUserId, IReadOnlyCollection<string> roles, int? branchId, string? status, bool includeDeleted);
    Task<ServiceResult<VehicleAssetDto>> GetVehicleByIdAsync(int actorUserId, IReadOnlyCollection<string> roles, int id);
    Task<ServiceResult<VehicleAssetDto>> CreateVehicleAsync(int actorUserId, IReadOnlyCollection<string> roles, VehicleCreateRequestDto request);
    Task<ServiceResult<VehicleAssetDto>> UpdateVehicleAsync(int actorUserId, IReadOnlyCollection<string> roles, int id, VehicleUpdateRequestDto request);

    // Dropdown data
    Task<ServiceResult<List<VehicleModel>>> GetModelsAsync();
    Task<ServiceResult<List<Driver>>> GetDriversAsync();
    Task<ServiceResult<List<Branch>>> GetBranchesAsync();

    // Assign / Unassign
    Task<ServiceResult<VehicleAssetDto>> AssignVehicleAsync(int actorUserId, IReadOnlyCollection<string> roles, int vehicleId, VehicleAssignRequestDto request);
    Task<ServiceResult<VehicleAssetDto>> UnassignVehicleAsync(int actorUserId, IReadOnlyCollection<string> roles, int vehicleId, VehicleUnassignRequestDto request);

    // Asset create (extended)
    Task<ServiceResult<VehicleAssetDto>> CreateAssetAsync(int actorUserId, IReadOnlyCollection<string> roles, AssetCreateRequestDto request);
}
