using Microsoft.EntityFrameworkCore.Storage;
using Models.DTO.DisposalProposals;
using Models.Models;

namespace Data.Repositories.DisposalProposals.Interfaces;

public interface IDisposalProposalRepository
{
    Task<(List<DisposalProposalDto> Items, int TotalCount)> GetListAsync(
        string? status,
        int? vehicleId,
        int? proposerId,
        DateOnly? fromDate,
        DateOnly? toDate,
        int? restrictedBranchId,
        int page,
        int pageSize);

    Task<DisposalProposalDto?> GetByIdAsync(int id, int? restrictedBranchId);
    Task<List<DisposalProposalDto>> GetByVehicleIdAsync(int vehicleId, int? restrictedBranchId);
    Task<DisposalProposal?> GetEntityByIdAsync(int id);
    Task<Vehicle?> GetVehicleByIdAsync(int vehicleId);
    Task<int?> GetUserBranchIdAsync(int userId);
    Task<bool> UserExistsAsync(int userId);
    Task<bool> HasPendingProposalAsync(int vehicleId);
    Task AddProposalAsync(DisposalProposal proposal);
    Task AddAssetChangeLogAsync(AssetChangeLog log);
    Task<IDbContextTransaction> BeginTransactionAsync();
    Task SaveChangesAsync();
}
