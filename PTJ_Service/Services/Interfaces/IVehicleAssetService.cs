using System.Collections.Generic;
using System.Threading.Tasks;
using Models.DTO.Vehicles;
using Service.Services;

namespace Service.Services.Interfaces;

public interface IVehicleAssetService
{
    Task<List<VehicleAssetDto>> GetVehiclesAsync(int? branchId, string? status, bool includeDeleted);
    Task<VehicleAssetDto?> GetVehicleByIdAsync(int id);
    Task<ServiceResult<VehicleAssetDto>> CreateVehicleAsync(VehicleCreateRequestDto request);
}
