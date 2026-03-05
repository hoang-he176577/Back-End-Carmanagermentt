using System.Collections.Generic;
using System.Threading.Tasks;
using Models.DTO.Vehicles;
using Service.Services;
using Service.Services.Common;

namespace Service.Services.Interfaces;

public interface IVehicleAssetService
{
    Task<List<VehicleAssetDto>> GetVehiclesAsync(int? branchId, string? status, bool includeDeleted);
    Task<VehicleAssetDto?> GetVehicleByIdAsync(int id);


    // Tạo mới asset đầy đủ (>= 15 field) chỉ cho Accountant
    Task<ServiceResult<VehicleAssetDto>> CreateAssetAsync(VehicleAssetCreateRequestDto request, int accountantUserId);

    // Phân bổ xe cho tài xế/người dùng
    Task<ServiceResult<VehicleAssetDto>> AssignVehicleAsync(int vehicleId, VehicleAssignRequestDto request, int performedByUserId);

    // Huỷ phân bổ, trả xe về trạng thái Available
    Task<ServiceResult<VehicleAssetDto>> UnassignVehicleAsync(int vehicleId, VehicleUnassignRequestDto request, int performedByUserId);
}
