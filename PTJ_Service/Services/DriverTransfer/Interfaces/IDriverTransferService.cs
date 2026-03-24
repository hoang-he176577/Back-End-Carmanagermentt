using Models.DTO.DriverTransfer;
using Service.Services.Common;

namespace Service.Services.DriverTransfer.Interfaces;

public interface IDriverTransferService
{
    Task<ServiceResult<List<DriverTransferRequestResponseDto>>> GetAllAsync(int actorUserId, IReadOnlyCollection<string> roles, string? status);
    Task<ServiceResult<DriverTransferRequestResponseDto>> GetByIdAsync(int actorUserId, IReadOnlyCollection<string> roles, int id);
    Task<ServiceResult<DriverTransferRequestResponseDto>> CreateAsync(int actorUserId, IReadOnlyCollection<string> roles, CreateDriverTransferRequestDto request);
    Task<ServiceResult<DriverTransferRequestResponseDto>> ConfirmAsync(int actorUserId, IReadOnlyCollection<string> roles, int id, ConfirmDriverTransferDto request);
    Task<ServiceResult<DriverTransferRequestResponseDto>> CancelAsync(int actorUserId, IReadOnlyCollection<string> roles, int id);
}
