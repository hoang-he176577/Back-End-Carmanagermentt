using Models.DTO.Accessories;
using Service.Services.Common;

namespace Service.Services.Accessories.Interfaces;

public interface IAccessoryService
{
    Task<ServiceResult<List<AccessoryDto>>> GetAccessoriesAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        string? keyword,
        string? type,
        bool? isActive,
        int? page,
        int? pageSize);

    Task<ServiceResult<AccessoryDto>> GetAccessoryByIdAsync(int actorUserId, IReadOnlyCollection<string> roles, int id);
    Task<ServiceResult<AccessoryDto>> CreateAccessoryAsync(int actorUserId, IReadOnlyCollection<string> roles, AccessoryCreateRequestDto request);
    Task<ServiceResult<AccessoryDto>> UpdateAccessoryAsync(int actorUserId, IReadOnlyCollection<string> roles, int id, AccessoryUpdateRequestDto request);
    Task<ServiceResult<AccessoryDto>> ImportAccessoryAsync(int actorUserId, IReadOnlyCollection<string> roles, int id, AccessoryImportRequestDto request);

    Task<ServiceResult<IssueVehicleAccessoryResponseDto>> IssueVehicleAccessoryAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        IssueVehicleAccessoryRequestDto request);

    Task<ServiceResult<VehicleAccessoryDto>> HandleVehicleAccessoryActionAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        int vehicleAccessoryId,
        ReturnVehicleAccessoryRequestDto request);

    Task<ServiceResult<List<VehicleAccessoryDto>>> GetVehicleAccessoriesAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        int vehicleId,
        bool activeOnly);

    Task<ServiceResult<List<AccessoryTransactionDto>>> GetTransactionsAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        int? accessoryId,
        int? vehicleId,
        string? transactionType,
        DateTime? fromDate,
        DateTime? toDate,
        int? page,
        int? pageSize);
}
