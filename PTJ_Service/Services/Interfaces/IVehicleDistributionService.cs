using System.Collections.Generic;
using System.Threading.Tasks;
using Models.DTO.VehicleDistribution;
using Service.Services.Common;

namespace Service.Services.Interfaces;

public interface IVehicleDistributionService
{
    Task<List<TransferPlanDto>> GetTransferPlansAsync(int? fromBranchId, int? toBranchId, string? status);
    Task<TransferPlanDto?> GetTransferPlanByIdAsync(int id);
    Task<ServiceResult<TransferPlanDto>> CreateTransferPlanAsync(TransferPlanCreateRequestDto request, int managerId);
    Task<ServiceResult<TransferPlanDto>> UpdateTransferPlanStatusAsync(int id, TransferPlanUpdateStatusDto request, int userId, string userRole);
    Task<List<BranchStockSummaryDto>> GetBranchStockAsync();
}
