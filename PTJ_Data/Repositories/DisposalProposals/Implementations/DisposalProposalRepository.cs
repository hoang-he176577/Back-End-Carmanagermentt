using Data.Repositories.DisposalProposals.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Models.DTO.DisposalProposals;
using Models.Models;
using System.Linq.Expressions;

namespace Data.Repositories.DisposalProposals.Implementations;

public sealed class DisposalProposalRepository : IDisposalProposalRepository
{
    private readonly CarManagerContext _context;

    private static readonly Expression<Func<DisposalProposal, DisposalProposalDto>> DisposalSelector = proposal => new DisposalProposalDto
    {
        Id = proposal.Id,
        VehicleId = proposal.VehicleId,
        VehicleLicensePlate = proposal.Vehicle != null ? proposal.Vehicle.LicensePlate : null,
        ProposerId = proposal.ProposerId,
        ProposerName = proposal.Proposer != null ? proposal.Proposer.Name : null,
        ManagerId = proposal.ManagerId,
        ManagerName = proposal.Manager != null ? proposal.Manager.Name : null,
        CreatedDate = proposal.CreatedDate,
        ApprovedDate = proposal.ApprovedDate,
        Status = proposal.Status,
        ProposedPrice = proposal.ProposedPrice,
        Reason = proposal.Reason,
        CreatedAt = proposal.CreatedAt,
        UpdatedAt = proposal.UpdatedAt
    };

    public DisposalProposalRepository(CarManagerContext context)
    {
        _context = context;
    }

    public async Task<(List<DisposalProposalDto> Items, int TotalCount)> GetListAsync(
        string? status,
        int? vehicleId,
        int? proposerId,
        DateOnly? fromDate,
        DateOnly? toDate,
        int? restrictedBranchId,
        int page,
        int pageSize)
    {
        var query = _context.DisposalProposals
            .AsNoTracking()
            .Where(x => x.DeletedAt == null);

        if (!string.IsNullOrWhiteSpace(status))
        {
            var normalizedStatus = status.Trim();
            query = query.Where(x => x.Status == normalizedStatus);
        }

        if (vehicleId.HasValue)
        {
            query = query.Where(x => x.VehicleId == vehicleId.Value);
        }

        if (proposerId.HasValue)
        {
            query = query.Where(x => x.ProposerId == proposerId.Value);
        }

        if (fromDate.HasValue)
        {
            query = query.Where(x => x.CreatedDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            query = query.Where(x => x.CreatedDate <= toDate.Value);
        }

        if (restrictedBranchId.HasValue)
        {
            var branchId = restrictedBranchId.Value;
            query = query.Where(x => x.Vehicle != null && x.Vehicle.CurrentBranchId == branchId);
        }

        var totalCount = await query.CountAsync();

        var items = await query
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(DisposalSelector)
            .ToListAsync();

        return (items, totalCount);
    }

    public Task<DisposalProposalDto?> GetByIdAsync(int id, int? restrictedBranchId)
    {
        var query = _context.DisposalProposals
            .AsNoTracking()
            .Where(x => x.Id == id && x.DeletedAt == null);

        if (restrictedBranchId.HasValue)
        {
            var branchId = restrictedBranchId.Value;
            query = query.Where(x => x.Vehicle != null && x.Vehicle.CurrentBranchId == branchId);
        }

        return query.Select(DisposalSelector).FirstOrDefaultAsync();
    }

    public Task<List<DisposalProposalDto>> GetByVehicleIdAsync(int vehicleId, int? restrictedBranchId)
    {
        var query = _context.DisposalProposals
            .AsNoTracking()
            .Where(x => x.VehicleId == vehicleId && x.DeletedAt == null);

        if (restrictedBranchId.HasValue)
        {
            var branchId = restrictedBranchId.Value;
            query = query.Where(x => x.Vehicle != null && x.Vehicle.CurrentBranchId == branchId);
        }

        return query
            .OrderByDescending(x => x.CreatedAt)
            .ThenByDescending(x => x.Id)
            .Select(DisposalSelector)
            .ToListAsync();
    }

    public Task<DisposalProposal?> GetEntityByIdAsync(int id)
    {
        return _context.DisposalProposals
            .Include(x => x.Vehicle)
            .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null);
    }

    public Task<Vehicle?> GetVehicleByIdAsync(int vehicleId)
    {
        return _context.Vehicles.FirstOrDefaultAsync(v => v.Id == vehicleId && v.DeletedAt == null);
    }

    public Task<int?> GetUserBranchIdAsync(int userId)
    {
        return _context.Users.AsNoTracking()
            .Where(u => u.Id == userId && u.DeletedAt == null)
            .Select(u => u.BranchId)
            .FirstOrDefaultAsync();
    }

    public Task<bool> UserExistsAsync(int userId)
    {
        return _context.Users.AsNoTracking().AnyAsync(u => u.Id == userId && u.DeletedAt == null);
    }

    public Task<bool> HasPendingProposalAsync(int vehicleId)
    {
        return _context.DisposalProposals.AsNoTracking()
            .AnyAsync(x =>
                x.VehicleId == vehicleId &&
                x.DeletedAt == null &&
                x.Status == "Pending");
    }

    public Task AddProposalAsync(DisposalProposal proposal)
    {
        _context.DisposalProposals.Add(proposal);
        return Task.CompletedTask;
    }

    public Task AddAssetChangeLogAsync(AssetChangeLog log)
    {
        _context.AssetChangeLogs.Add(log);
        return Task.CompletedTask;
    }

    public Task<IDbContextTransaction> BeginTransactionAsync()
    {
        return _context.Database.BeginTransactionAsync();
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }
}
