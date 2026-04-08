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

    Task<ServiceResult<List<BranchAccessoryStockDto>>> GetBranchStocksAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        int? branchId,
        int? accessoryId,
        bool belowMinimumOnly,
        int? page,
        int? pageSize);

    Task<ServiceResult<BranchAccessoryStockDto>> UpsertBranchStockAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        BranchAccessoryStockUpsertRequestDto request);

    Task<ServiceResult<List<AccessoryPurchaseRequestDto>>> GetPurchaseRequestsAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        int? branchId,
        string? status,
        int? page,
        int? pageSize);

    Task<ServiceResult<AccessoryPurchaseRequestDto>> GetPurchaseRequestByIdAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        int id);

    Task<ServiceResult<AccessoryPurchaseRequestDto>> CreatePurchaseRequestAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        AccessoryPurchaseRequestCreateRequestDto request);

    Task<ServiceResult<AccessoryPurchaseRequestDto>> UpdatePurchaseRequestAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        int id,
        AccessoryPurchaseRequestUpdateRequestDto request);

    Task<ServiceResult<bool>> DeletePurchaseRequestAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        int id);

    Task<ServiceResult<AccessoryPurchaseRequestDto>> ApprovePurchaseRequestAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        int id,
        AccessoryPurchaseRequestApproveRequestDto request);

    Task<ServiceResult<AccessoryPurchaseRequestDto>> RejectPurchaseRequestAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        int id,
        AccessoryPurchaseRequestRejectRequestDto request);

    Task<ServiceResult<List<AccessoryGoodsReceiptDto>>> GetGoodsReceiptsAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        int? branchId,
        int? purchaseRequestId,
        string? status,
        int? page,
        int? pageSize);

    Task<ServiceResult<AccessoryGoodsReceiptDto>> GetGoodsReceiptByIdAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        int id);

    Task<ServiceResult<AccessoryGoodsReceiptDto>> CreateGoodsReceiptAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        AccessoryGoodsReceiptCreateRequestDto request);

    Task<ServiceResult<AccessoryGoodsReceiptDto>> CompleteGoodsReceiptAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        int id,
        AccessoryGoodsReceiptCompleteRequestDto request);

    Task<ServiceResult<AccessoryGoodsReceiptDto>> CancelGoodsReceiptAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        int id,
        string? notes);

    Task<ServiceResult<List<VehicleAccessoryRequirementDto>>> GetVehicleAccessoryRequirementsAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        int? modelId);

    Task<ServiceResult<VehicleAccessoryRequirementDto>> CreateVehicleAccessoryRequirementAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        VehicleAccessoryRequirementUpsertRequestDto request);

    Task<ServiceResult<VehicleAccessoryRequirementDto>> UpdateVehicleAccessoryRequirementAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        int id,
        VehicleAccessoryRequirementUpsertRequestDto request);

    Task<ServiceResult<bool>> DeleteVehicleAccessoryRequirementAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        int id);

    Task<ServiceResult<VehicleAccessoryRequirementCheckResultDto>> CheckVehicleAccessoryRequirementsAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        int vehicleId);

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
        int? branchId,
        int? accessoryId,
        int? vehicleId,
        string? transactionType,
        string? referenceType,
        DateTime? fromDate,
        DateTime? toDate,
        int? page,
        int? pageSize);
}
