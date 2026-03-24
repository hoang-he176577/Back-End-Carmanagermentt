using System.Linq.Expressions;
using Data.Repositories.DriverTransfer.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models.DTO.DriverTransfer;
using Models.Models;

namespace Data.Repositories.DriverTransfer.Implementations;

public sealed class DriverTransferRepository : IDriverTransferRepository
{
    private readonly CarManagerContext _context;

    private static readonly Expression<Func<DriverTransferRequest, DriverTransferRequestResponseDto>> RequestSelector =
        r => new DriverTransferRequestResponseDto
        {
            Id = r.Id,
            RequestingBranchId = r.RequestingBranchId,
            RequestingBranchName = r.RequestingBranch != null ? r.RequestingBranch.Name : null,
            RequestedQuantity = r.RequestedQuantity,
            FulfilledQuantity = r.FulfilledQuantity,
            Status = r.Status,
            Reason = r.Reason,
            CreatedByUserId = r.CreatedByUserId,
            CreatedByUserName = r.CreatedByUser != null ? r.CreatedByUser.Name : null,
            CreatedAt = r.CreatedAt,
            UpdatedAt = r.UpdatedAt,
            TransferDetails = r.TransferDetails.Select(d => new DriverTransferDetailResponseDto
            {
                Id = d.Id,
                DriverId = d.DriverId,
                DriverName = d.Driver != null ? d.Driver.Name : null,
                DriverLicenseNumber = d.Driver != null ? d.Driver.LicenseNumber : null,
                DriverPhone = d.Driver != null ? d.Driver.Phone : null,
                FromBranchId = d.FromBranchId,
                FromBranchName = d.FromBranch != null ? d.FromBranch.Name : null,
                ConfirmedByUserId = d.ConfirmedByUserId,
                ConfirmedByUserName = d.ConfirmedByUser != null ? d.ConfirmedByUser.Name : null,
                TransferDate = d.TransferDate,
                CreatedAt = d.CreatedAt
            }).ToList()
        };

    public DriverTransferRepository(CarManagerContext context)
    {
        _context = context;
    }

    public Task<List<DriverTransferRequestResponseDto>> GetAllRequestsAsync(string? status)
    {
        var query = _context.DriverTransferRequests.AsNoTracking().Where(r => r.DeletedAt == null);
        if (!string.IsNullOrWhiteSpace(status))
        {
            var targetStatus = status.Trim();
            query = query.Where(r => r.Status == targetStatus);
        }

        return query.OrderByDescending(r => r.Id).Select(RequestSelector).ToListAsync();
    }

    public Task<DriverTransferRequestResponseDto?> GetRequestByIdAsync(int id)
    {
        return _context.DriverTransferRequests.AsNoTracking()
            .Where(r => r.Id == id && r.DeletedAt == null)
            .Select(RequestSelector)
            .FirstOrDefaultAsync();
    }

    public Task<DriverTransferRequest?> GetRequestEntityByIdAsync(int id)
    {
        return _context.DriverTransferRequests
            .FirstOrDefaultAsync(r => r.Id == id && r.DeletedAt == null);
    }

    public async Task<DriverTransferRequest> CreateRequestAsync(DriverTransferRequest entity)
    {
        _context.DriverTransferRequests.Add(entity);
        await _context.SaveChangesAsync();
        return entity;
    }

    public Task UpdateRequestAsync(DriverTransferRequest entity)
    {
        _context.DriverTransferRequests.Update(entity);
        return _context.SaveChangesAsync();
    }

    public async Task AddTransferDetailsAsync(List<DriverTransferDetail> details)
    {
        _context.DriverTransferDetails.AddRange(details);
        await _context.SaveChangesAsync();
    }

    public Task<int?> GetUserBranchIdAsync(int userId)
    {
        return _context.Users.AsNoTracking()
            .Where(u => u.Id == userId && u.DeletedAt == null)
            .Select(u => u.BranchId)
            .FirstOrDefaultAsync();
    }

    public Task<bool> BranchExistsAsync(int branchId)
    {
        return _context.Branches.AsNoTracking()
            .AnyAsync(b => b.Id == branchId && b.DeletedAt == null);
    }

    public Task<List<Driver>> GetDriverEntitiesByIdsAsync(List<int> driverIds)
    {
        return _context.Drivers
            .Where(d => d.DeletedAt == null && driverIds.Contains(d.Id))
            .ToListAsync();
    }
}
