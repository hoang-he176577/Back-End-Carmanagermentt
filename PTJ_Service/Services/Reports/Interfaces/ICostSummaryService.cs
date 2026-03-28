using Models.DTO.Reports;

namespace Service.Services.Reports.Interfaces
{
    public interface ICostSummaryService
    {
        Task<EstimatedCostSummaryDto> GetEstimatedCostsAsync(int? branchId);
        Task<EstimatedCostReportDto> GetEstimatedCostsByBranchAsync();
    }
}
