using Models.DTO.Maintenance;
using Models.Models;

namespace Data.Repositories.MaintenanceRequests.Interfaces;

public interface IMaintenanceRequestRepository
{
    Task<List<MaintenanceRequestDto>> GetListAsync(string? status, string? maintenanceType, bool includeDeleted, int? branchId = null);
    Task<List<string>> GetDistinctMaintenanceTypesAsync(bool includeDeleted = true);
    Task<MaintenanceRequestDto?> GetByIdAsync(int id, bool includeDeleted = false);
    Task<MaintenanceRequest?> GetEntityByIdAsync(int id, bool includeDeleted = false);
    Task<bool> VehicleExistsAsync(int vehicleId);
    Task<bool> UserExistsAsync(int userId);
    Task<int?> GetUserBranchIdAsync(int userId);
    Task<MaintenanceRequest> AddAsync(MaintenanceRequest entity);
    Task SaveChangesAsync();
}
