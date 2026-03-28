using Microsoft.EntityFrameworkCore;
using Models.DTO.Reports;
using Models.Models;
using Service.Services.Reports.Interfaces;

namespace Service.Services.Reports.Implementations
{
    public sealed class CostSummaryService : ICostSummaryService
    {
        private readonly CarManagerContext _context;

        public CostSummaryService(CarManagerContext context)
        {
            _context = context;
        }

        public async Task<EstimatedCostSummaryDto> GetEstimatedCostsAsync(int? branchId)
        {
            var maintenanceQuery = _context.MaintenanceRequests
                .AsNoTracking()
                .Where(x => x.DeletedAt == null);

            if (branchId.HasValue)
            {
                maintenanceQuery = maintenanceQuery.Where(x =>
                    x.Vehicle != null && x.Vehicle.CurrentBranchId == branchId.Value);
            }

            var maintenanceCost = await maintenanceQuery
                .SumAsync(x => x.EstimatedCost ?? 0m);

            var purchaseDetailQuery = _context.BulkPurchaseDetails
                .AsNoTracking()
                .Where(x => x.PurchaseProposal != null && x.PurchaseProposal.DeletedAt == null);

            if (branchId.HasValue)
            {
                purchaseDetailQuery = purchaseDetailQuery.Where(x => x.BranchId == branchId.Value);
            }

            var vehiclePurchaseCost = await purchaseDetailQuery
                .SumAsync(x =>
                    (x.ProposedQuantity ?? 0) * (x.UnitPrice ?? 0m)
                    + (x.RegistrationTax ?? 0m)
                    + (x.RoadMaintenanceFee ?? 0m)
                    + (x.LicensePlateFee ?? 0m)
                    + (x.InsuranceFee ?? 0m));

            var accessoryQuery = _context.AccessoryPurchaseRequestDetails
                .AsNoTracking()
                .Where(x => x.Request != null);

            if (branchId.HasValue)
            {
                accessoryQuery = accessoryQuery.Where(x => x.Request.BranchId == branchId.Value);
            }

            var accessoryPurchaseCost = await accessoryQuery
                .SumAsync(x => (x.RequestedQuantity) * (x.EstimatedUnitPrice ?? 0m));

            var disposalQuery = _context.DisposalProposals
                .AsNoTracking()
                .Where(x => x.DeletedAt == null);

            if (branchId.HasValue)
            {
                disposalQuery = disposalQuery.Where(x =>
                    x.Vehicle != null && x.Vehicle.CurrentBranchId == branchId.Value);
            }

            var disposalCost = await disposalQuery
                .SumAsync(x => x.ProposedPrice ?? 0m);

            string? branchName = null;
            if (branchId.HasValue)
            {
                branchName = await _context.Branches
                    .AsNoTracking()
                    .Where(b => b.Id == branchId.Value)
                    .Select(b => b.Name)
                    .FirstOrDefaultAsync();
            }

            var total = maintenanceCost + vehiclePurchaseCost + accessoryPurchaseCost + disposalCost;

            return new EstimatedCostSummaryDto
            {
                BranchId = branchId,
                BranchName = branchName,
                MaintenanceCost = maintenanceCost,
                VehiclePurchaseCost = vehiclePurchaseCost,
                AccessoryPurchaseCost = accessoryPurchaseCost,
                DisposalCost = disposalCost,
                TotalCost = total
            };
        }

        public async Task<EstimatedCostReportDto> GetEstimatedCostsByBranchAsync()
        {
            var branches = await _context.Branches.AsNoTracking()
                .Select(b => new { b.Id, b.Name })
                .ToListAsync();

            var branchSummaries = new List<EstimatedCostBranchSummaryDto>();
            foreach (var branch in branches)
            {
                var summary = await GetEstimatedCostsAsync(branch.Id);
                branchSummaries.Add(new EstimatedCostBranchSummaryDto
                {
                    BranchId = branch.Id,
                    BranchName = branch.Name,
                    MaintenanceCost = summary.MaintenanceCost,
                    VehiclePurchaseCost = summary.VehiclePurchaseCost,
                    AccessoryPurchaseCost = summary.AccessoryPurchaseCost,
                    DisposalCost = summary.DisposalCost,
                    TotalCost = summary.TotalCost
                });
            }

            var total = await GetEstimatedCostsAsync(null);

            return new EstimatedCostReportDto
            {
                Branches = branchSummaries,
                Total = total
            };
        }
    }
}
