using Models.DTO.Maintenance;
using Service.Services.Common;

namespace Service.Services.MaintenanceRequests.Interfaces;

public interface IMaintenanceRequestService
{
    Task<ServiceResult<List<MaintenanceRequestDto>>> GetListAsync(string? status, string? maintenanceType, bool includeDeleted, int userId, string userRole);
    Task<ServiceResult<MaintenanceRequestDto>> GetByIdAsync(int id, bool includeDeleted);
    Task<ServiceResult<MaintenanceRequestDto>> CreateAsync(int actorUserId, MaintenanceCreateRequestDto request);
    Task<ServiceResult<MaintenanceRequestDto>> UpdateAsync(int id, MaintenanceUpdateRequestDto request);
    Task<ServiceResult<MaintenanceRequestDto>> ApproveOrRejectAsync(int id, int accountantUserId, MaintenanceApprovalRequestDto request);
    Task<ServiceResult<bool>> DeleteAsync(int id);
}
