using Models.DTO.DriverTransfer;
using Models.Models;

namespace Data.Repositories.DriverTransfer.Interfaces;

public interface IDriverTransferRepository
{
    Task<List<DriverTransferRequestResponseDto>> GetAllRequestsAsync(string? status);
    Task<DriverTransferRequestResponseDto?> GetRequestByIdAsync(int id);
    Task<DriverTransferRequest?> GetRequestEntityByIdAsync(int id);
    Task<DriverTransferRequest> CreateRequestAsync(DriverTransferRequest entity);
    Task UpdateRequestAsync(DriverTransferRequest entity);
    Task AddTransferDetailsAsync(List<DriverTransferDetail> details);
    Task<int?> GetUserBranchIdAsync(int userId);
    Task<bool> BranchExistsAsync(int branchId);
    Task<List<Driver>> GetDriverEntitiesByIdsAsync(List<int> driverIds);
}
