using Data.Repositories.MaintenanceRequests.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models.DTO.Maintenance;
using Models.Models;

namespace Data.Repositories.MaintenanceRequests.Implementations;

public sealed class MaintenanceRequestRepository : IMaintenanceRequestRepository
{
    private readonly CarManagerContext _context;

    public MaintenanceRequestRepository(CarManagerContext context)
    {
        _context = context;
    }

    public async Task<List<MaintenanceRequestDto>> GetListAsync(string? status, string? maintenanceType, bool includeDeleted, int? branchId = null, int? vehicleId = null)
    {
        var query = _context.MaintenanceRequests.AsQueryable();

        if (!includeDeleted)
        {
            query = query.Where(x => x.DeletedAt == null);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = status.Trim();
            query = query.Where(x => x.Status != null && x.Status == normalizedStatus);
        }

        if (!string.IsNullOrWhiteSpace(maintenanceType))
        {
            var normalizedType = maintenanceType.Trim();
            query = query.Where(x => x.MaintenanceType == normalizedType);
        }

        if (vehicleId.HasValue)
        {
            query = query.Where(x => x.VehicleId == vehicleId.Value);
        }

        // Filter by branch: join with Vehicle to check CurrentBranchId
        if (branchId.HasValue)
        {
            query = query.Where(x => x.Vehicle != null && x.Vehicle.CurrentBranchId == branchId.Value);
        }

        return await query
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new MaintenanceRequestDto
            {
                Id = x.Id,
                VehicleId = x.VehicleId,
                VehicleLicensePlate = x.Vehicle != null ? x.Vehicle.LicensePlate : null,
                VehicleModelName = x.Vehicle != null && x.Vehicle.Model != null
                    ? ((x.Vehicle.Model.Manufacturer ?? "") + " " + (x.Vehicle.Model.ModelName ?? "")).Trim()
                    : null,
                OperatorId = x.OperatorId,
                RequestDate = x.RequestDate,
                Description = x.Description,
                EstimatedCost = x.EstimatedCost,
                Status = x.Status,
                MaintenanceType = x.MaintenanceType,
                AccountantId = x.AccountantId,
                ApproverName = x.Accountant != null ? x.Accountant.Name : null,
                ApprovedDate = x.ApprovedDate,
                ActualCost = x.ActualCost,
                CompletionDate = x.CompletionDate,
                CompletionNote = x.CompletionNote,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt,
                DeletedAt = x.DeletedAt
            })
            .ToListAsync();
    }

    public async Task<MaintenanceRequestDto?> GetByIdAsync(int id, bool includeDeleted = false)
    {
        var query = _context.MaintenanceRequests.Where(x => x.Id == id);
        if (!includeDeleted)
        {
            query = query.Where(x => x.DeletedAt == null);
        }

        return await query.Select(x => new MaintenanceRequestDto
        {
            Id = x.Id,
            VehicleId = x.VehicleId,
            VehicleLicensePlate = x.Vehicle != null ? x.Vehicle.LicensePlate : null,
            VehicleModelName = x.Vehicle != null && x.Vehicle.Model != null
                ? ((x.Vehicle.Model.Manufacturer ?? "") + " " + (x.Vehicle.Model.ModelName ?? "")).Trim()
                : null,
            OperatorId = x.OperatorId,
            RequestDate = x.RequestDate,
            Description = x.Description,
            EstimatedCost = x.EstimatedCost,
            Status = x.Status,
            MaintenanceType = x.MaintenanceType,
            AccountantId = x.AccountantId,
            ApproverName = x.Accountant != null ? x.Accountant.Name : null,
            ApprovedDate = x.ApprovedDate,
            ActualCost = x.ActualCost,
            CompletionDate = x.CompletionDate,
            CompletionNote = x.CompletionNote,
            CreatedAt = x.CreatedAt,
            UpdatedAt = x.UpdatedAt,
            DeletedAt = x.DeletedAt
        }).FirstOrDefaultAsync();
    }

    public async Task<MaintenanceRequest?> GetEntityByIdAsync(int id, bool includeDeleted = false)
    {
        var query = _context.MaintenanceRequests.Where(x => x.Id == id);
        if (!includeDeleted)
        {
            query = query.Where(x => x.DeletedAt == null);
        }

        return await query
            .Include(x => x.Vehicle)
            .FirstOrDefaultAsync();
    }

    public Task<bool> VehicleExistsAsync(int vehicleId)
        => _context.Vehicles.AnyAsync(v => v.Id == vehicleId && v.DeletedAt == null);

    public Task<string?> GetVehicleStatusAsync(int vehicleId)
        => _context.Vehicles.Where(v => v.Id == vehicleId && v.DeletedAt == null)
            .Select(v => v.Status).FirstOrDefaultAsync();

    public Task<bool> UserExistsAsync(int userId)
        => _context.Users.AnyAsync(u => u.Id == userId && u.DeletedAt == null);

    public async Task<MaintenanceRequest> AddAsync(MaintenanceRequest entity)
    {
        _context.MaintenanceRequests.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public async Task<int?> GetUserBranchIdAsync(int userId)
    {
        return await _context.Users.AsNoTracking()
            .Where(u => u.Id == userId)
            .Select(u => u.BranchId)
            .FirstOrDefaultAsync();
    }

    public Task SaveChangesAsync()
        => _context.SaveChangesAsync();
}
