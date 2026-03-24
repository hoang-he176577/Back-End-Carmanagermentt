using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models.DTO.VehicleDistribution;
using Models.Models;

namespace Data.Repositories.Implementations;

public sealed class VehicleDistributionRepository : IVehicleDistributionRepository
{
    private readonly CarManagerContext _context;

    private static readonly Expression<Func<TransferPlan, TransferPlanDto>> TransferPlanSelector = t => new TransferPlanDto
    {
        Id = t.Id,
        VehicleId = t.VehicleId,
        LicensePlate = t.Vehicle != null ? t.Vehicle.LicensePlate : null,
        FromBranchId = t.FromBranchId,
        FromBranchName = t.FromBranch != null ? t.FromBranch.Name : null,
        ToBranchId = t.ToBranchId,
        ToBranchName = t.ToBranch != null ? t.ToBranch.Name : null,
        ManagerId = t.ManagerId,
        ManagerName = t.Manager != null ? t.Manager.Name : null,
        PlanDate = t.PlanDate,
        ExecutedDate = t.ExecutedDate,
        Status = t.Status,
        CheckoutDate = t.CheckoutDate,
        CheckoutByName = t.CheckoutByUser != null ? t.CheckoutByUser.Name : null,
        CheckinDate = t.CheckinDate,
        CheckinByName = t.CheckinByUser != null ? t.CheckinByUser.Name : null,
        CreatedAt = t.CreatedAt
    };

    public VehicleDistributionRepository(CarManagerContext context)
    {
        _context = context;
    }

    // ───────────────────────────── Transfer Plans ─────────────────────────────

    public async Task<List<TransferPlanDto>> GetTransferPlansAsync(
        int? fromBranchId, int? toBranchId, string? status, int? userBranchId = null)
    {
        var query = _context.TransferPlans.AsNoTracking()
            .Where(t => t.DeletedAt == null);

        if (fromBranchId.HasValue)
            query = query.Where(t => t.FromBranchId == fromBranchId.Value);

        if (toBranchId.HasValue)
            query = query.Where(t => t.ToBranchId == toBranchId.Value);

        if (!string.IsNullOrWhiteSpace(status))
        {
            var trimmed = status.Trim();
            query = query.Where(t => t.Status == trimmed);
        }

        // Filter by user's branch: show transfers FROM or TO user's branch
        if (userBranchId.HasValue)
        {
            query = query.Where(t => t.FromBranchId == userBranchId.Value || t.ToBranchId == userBranchId.Value);
        }

        return await query
            .OrderByDescending(t => t.CreatedAt)
            .Select(TransferPlanSelector)
            .ToListAsync();
    }

    public async Task<TransferPlanDto?> GetTransferPlanByIdAsync(int id)
    {
        return await _context.TransferPlans.AsNoTracking()
            .Where(t => t.Id == id && t.DeletedAt == null)
            .Select(TransferPlanSelector)
            .FirstOrDefaultAsync();
    }

    public async Task<TransferPlan?> GetTransferPlanEntityAsync(int id)
    {
        return await _context.TransferPlans
            .FirstOrDefaultAsync(t => t.Id == id && t.DeletedAt == null);
    }

    public async Task<TransferPlan> AddTransferPlanAsync(TransferPlan plan)
    {
        _context.TransferPlans.Add(plan);
        await _context.SaveChangesAsync();
        return plan;
    }

    public async Task UpdateTransferPlanAsync(TransferPlan plan)
    {
        plan.UpdatedAt = DateTime.Now;
        _context.TransferPlans.Update(plan);
        await _context.SaveChangesAsync();
    }

    // ───────────────────────────── Branch Stock ─────────────────────────────

    public async Task<List<BranchStockSummaryDto>> GetBranchStockSummariesAsync()
    {
        var branches = await _context.Branches.AsNoTracking()
            .Where(b => b.DeletedAt == null)
            .Select(b => new BranchStockSummaryDto
            {
                BranchId = b.Id,
                BranchName = b.Name,
                TotalVehicles = b.Vehicles.Count(v => v.DeletedAt == null),
                ActiveVehicles = b.Vehicles.Count(v => v.DeletedAt == null && v.Status == "Active"),
                InTransferVehicles = b.TransferPlanFromBranches.Count(t => t.DeletedAt == null && (t.Status == "Pending" || t.Status == "InTransit")),
                Vehicles = b.Vehicles
                .Where(v => v.DeletedAt == null)
                .Select(v => new Models.DTO.Vehicles.VehicleAssetDto
                {
                    Id = v.Id,
                    LicensePlate = v.LicensePlate,
                    Manufacturer = v.Model != null ? v.Model.Manufacturer : "N/A",
                    ModelName = v.Model != null ? v.Model.ModelName : "N/A",
                    Status = v.Status,
                    CurrentBranchId = v.CurrentBranchId,
                    CurrentBranchName = b.Name // Tên chi nhánh hiện tại
                }).ToList() 
            })
            .ToListAsync();

        return branches;
    }

    // ───────────────────────────── Helpers ─────────────────────────────

    public Task<bool> VehicleExistsAsync(int vehicleId)
    {
        return _context.Vehicles.AsNoTracking()
            .AnyAsync(v => v.Id == vehicleId && v.DeletedAt == null);
    }

    public Task<bool> BranchExistsAsync(int branchId)
    {
        return _context.Branches.AsNoTracking()
            .AnyAsync(b => b.Id == branchId && b.DeletedAt == null);
    }

    public Task<bool> HasActiveTransferAsync(int vehicleId)
    {
        return _context.TransferPlans.AsNoTracking()
            .AnyAsync(t => t.VehicleId == vehicleId
                        && t.DeletedAt == null
                        && (t.Status == "Pending" || t.Status == "InTransit"));
    }

    public async Task UpdateVehicleBranchAsync(int vehicleId, int newBranchId)
    {
        var vehicle = await _context.Vehicles.FindAsync(vehicleId);
        if (vehicle != null)
        {
            vehicle.CurrentBranchId = newBranchId;
            vehicle.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateVehicleStatusAsync(int vehicleId, string status)
    {
        var vehicle = await _context.Vehicles.FindAsync(vehicleId);
        if (vehicle != null)
        {
            vehicle.Status = status;
            vehicle.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }

    public async Task UnassignVehicleDriverAsync(int vehicleId)
    {
        var vehicle = await _context.Vehicles.FindAsync(vehicleId);
        if (vehicle != null)
        {
            vehicle.CurrentDriverId = null;
            vehicle.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }

    public async Task<int?> GetUserBranchIdAsync(int userId)
    {
        return await _context.Users.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.BranchId)
            .FirstOrDefaultAsync();
    }
}
