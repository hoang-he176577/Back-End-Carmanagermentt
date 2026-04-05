using Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models.DTO.PendingRequests;
using Models.Models;

namespace Data.Repositories.Implementations;

public sealed class PendingRequestRepository : IPendingRequestRepository
{
    private readonly CarManagerContext _context;

    public PendingRequestRepository(CarManagerContext context)
    {
        _context = context;
    }

    public async Task<List<PendingRequestDto>> GetPendingRequestsAsync(
        string? status, DateTime? fromDate, DateTime? toDate)
    {
        var effectiveStatus = string.IsNullOrWhiteSpace(status) ? "Pending" : status.Trim();

        // ----- Purchase Proposals -----
        var purchaseQuery = _context.PurchaseProposals
            .Where(p => p.DeletedAt == null && p.Status == effectiveStatus);

        if (fromDate.HasValue)
            purchaseQuery = purchaseQuery.Where(p => p.CreatedAt >= fromDate.Value);
        if (toDate.HasValue)
            purchaseQuery = purchaseQuery.Where(p => p.CreatedAt <= toDate.Value);

        var purchases = await purchaseQuery
            .Select(p => new PendingRequestDto
            {
                Id = p.Id,
                Type = "Purchase",
                Status = p.Status,
                Description = p.Description,
                Amount = p.ProposedCost,
                ProposerName = p.Proposer != null ? p.Proposer.Name : null,
                RequestDate = p.CreatedDate,
                CreatedAt = p.CreatedAt,
                VehicleLicensePlate = null
            })
            .ToListAsync();

        // ----- Disposal Proposals -----
        var disposalQuery = _context.DisposalProposals
            .Where(d => d.DeletedAt == null && d.Status == effectiveStatus);

        if (fromDate.HasValue)
            disposalQuery = disposalQuery.Where(d => d.CreatedAt >= fromDate.Value);
        if (toDate.HasValue)
            disposalQuery = disposalQuery.Where(d => d.CreatedAt <= toDate.Value);

        var disposals = await disposalQuery
            .Select(d => new PendingRequestDto
            {
                Id = d.Id,
                Type = "Disposal",
                Status = d.Status,
                Description = d.Reason,
                Amount = d.ProposedPrice,
                ProposerName = d.Proposer != null ? d.Proposer.Name : null,
                RequestDate = d.CreatedDate,
                CreatedAt = d.CreatedAt,
                VehicleLicensePlate = d.Vehicle != null ? d.Vehicle.LicensePlate : null
            })
            .ToListAsync();

        // ----- Maintenance Requests -----
        var maintenanceQuery = _context.MaintenanceRequests
            .Where(m => m.DeletedAt == null && m.Status == effectiveStatus);

        if (fromDate.HasValue)
            maintenanceQuery = maintenanceQuery.Where(m => m.CreatedAt >= fromDate.Value);
        if (toDate.HasValue)
            maintenanceQuery = maintenanceQuery.Where(m => m.CreatedAt <= toDate.Value);

        var maintenances = await maintenanceQuery
            .Select(m => new PendingRequestDto
            {
                Id = m.Id,
                Type = "Maintenance",
                Status = m.Status,
                Description = m.Description,
                Amount = m.EstimatedCost,
                ProposerName = m.Operator != null ? m.Operator.Name : null,
                RequestDate = m.RequestDate,
                CreatedAt = m.CreatedAt,
                VehicleLicensePlate = m.Vehicle != null ? m.Vehicle.LicensePlate : null
            })
            .ToListAsync();

        // ----- Over Budget Repair Proposals -----
        var overBudgetQuery = _context.OverBudgetRepairProposals
            .Where(o => o.DeletedAt == null && o.Status == effectiveStatus);

        if (fromDate.HasValue)
            overBudgetQuery = overBudgetQuery.Where(o => o.CreatedAt >= fromDate.Value);
        if (toDate.HasValue)
            overBudgetQuery = overBudgetQuery.Where(o => o.CreatedAt <= toDate.Value);

        var overBudgets = await overBudgetQuery
            .Select(o => new PendingRequestDto
            {
                Id = o.Id,
                Type = "OverBudgetRepair",
                Status = o.Status,
                Description = o.Maintenance != null ? o.Maintenance.Description : null,
                Amount = o.OverBudgetAmount,
                ProposerName = null,
                RequestDate = null,
                CreatedAt = o.CreatedAt,
                VehicleLicensePlate = o.Maintenance != null && o.Maintenance.Vehicle != null
                    ? o.Maintenance.Vehicle.LicensePlate
                    : null
            })
            .ToListAsync();

        // ----- Transfer Plans -----
        var transferQuery = _context.TransferPlans
            .Where(t => t.DeletedAt == null && t.Status == effectiveStatus);

        if (fromDate.HasValue)
            transferQuery = transferQuery.Where(t => t.CreatedAt >= fromDate.Value);
        if (toDate.HasValue)
            transferQuery = transferQuery.Where(t => t.CreatedAt <= toDate.Value);

        var transfers = await transferQuery
            .Select(t => new PendingRequestDto
            {
                Id = t.Id,
                Type = "Transfer",
                Status = t.Status,
                Description = t.FromBranch != null && t.ToBranch != null
                    ? "Transfer from " + t.FromBranch.Name + " to " + t.ToBranch.Name
                    : null,
                Amount = null,
                ProposerName = t.Manager != null ? t.Manager.Name : null,
                RequestDate = t.PlanDate,
                CreatedAt = t.CreatedAt,
                VehicleLicensePlate = t.Vehicle != null ? t.Vehicle.LicensePlate : null
            })
            .ToListAsync();

        // ----- Merge & sort by CreatedAt descending -----
        var result = new List<PendingRequestDto>();
        result.AddRange(purchases);
        result.AddRange(disposals);
        result.AddRange(maintenances);
        result.AddRange(overBudgets);
        result.AddRange(transfers);

        return result.OrderByDescending(r => r.CreatedAt).ToList();
    }
}
