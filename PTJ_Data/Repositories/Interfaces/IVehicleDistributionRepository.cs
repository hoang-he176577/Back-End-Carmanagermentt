using System.Collections.Generic;
using System.Threading.Tasks;
using Models.DTO.VehicleDistribution;
using Models.Models;

namespace Data.Repositories.Interfaces;

public interface IVehicleDistributionRepository
{
    Task<List<TransferPlanDto>> GetTransferPlansAsync(int? fromBranchId, int? toBranchId, string? status, int? userBranchId = null);
    Task<TransferPlanDto?> GetTransferPlanByIdAsync(int id);
    Task<TransferPlan?> GetTransferPlanEntityAsync(int id);
    Task<TransferPlan> AddTransferPlanAsync(TransferPlan plan);
    Task UpdateTransferPlanAsync(TransferPlan plan);
    Task<List<BranchStockSummaryDto>> GetBranchStockSummariesAsync();
    Task<bool> VehicleExistsAsync(int vehicleId);
    Task<bool> BranchExistsAsync(int branchId);
    Task<bool> HasActiveTransferAsync(int vehicleId);
    Task UpdateVehicleBranchAsync(int vehicleId, int newBranchId);
    Task UpdateVehicleStatusAsync(int vehicleId, string status);
    Task UnassignVehicleDriverAsync(int vehicleId);
    Task<int?> GetUserBranchIdAsync(int userId);
}
