using Models.DTO.Drivers;
using Service.Services.Common;

namespace Service.Services.Drivers.Interfaces;

public interface IDriverService
{
    Task<ServiceResult<List<DriverResponseDto>>> GetAllAsync(int actorUserId, IReadOnlyCollection<string> roles, int? branchId);
    Task<ServiceResult<DriverResponseDto>> GetByIdAsync(int actorUserId, IReadOnlyCollection<string> roles, int id);
    Task<ServiceResult<DriverResponseDto>> CreateAsync(int actorUserId, IReadOnlyCollection<string> roles, DriverCreateDto request);
    Task<ServiceResult<DriverResponseDto>> UpdateAsync(int actorUserId, IReadOnlyCollection<string> roles, int id, DriverUpdateDto request);
    Task<ServiceResult<bool>> DeleteAsync(int actorUserId, IReadOnlyCollection<string> roles, int id);
    Task<ServiceResult<List<DriverDropdownDto>>> GetAvailableByBranchAsync(int actorUserId, IReadOnlyCollection<string> roles, int? branchId);
    Task<ServiceResult<List<DriverDropdownDto>>> GetDropdownAsync(int actorUserId, IReadOnlyCollection<string> roles, int? branchId);
}
