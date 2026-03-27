using Models.DTO.PurchaseProposal;
using Models.Models;
using System;


namespace Service.Services.Interfaces
{
    public interface IPurchaseProposalService
    {
        Task<List<PurchasePlanDto>> GetAllAsync();
        Task<PurchaseProposal?> GetByIdAsync(int id);
        Task<object> CreateAsync(CreatePurchaseProposalDto dto, int proposerId, int branchId);
        Task<object> UpdateAsync(int proposalId, UpdatePurchaseProposalDto dto, int proposerId, int branchId);
        Task ApproveByManagerAsync(int proposalId, int managerId);
        Task RejectAsync(int proposalId, string reason);
        Task DeleteAsync(int proposalId);

        Task<List<PurchaseProposal>> GetPendingForManagerAsync();

        Task<List<PurchaseProposalDto>> GetApprovedByBranchAsync(int branchId);

        /// <summary>
        /// Lấy danh sách kế hoạch mua (chỉ những đề xuất đã duyệt)
        /// Dùng cho hiển thị tab "Kế hoạch mua"
        /// </summary>
        Task<List<PurchasePlanDto>> GetPurchasePlanAsync(int? branchId = null);
        Task ConfirmPaymentAsync(int proposalId, int accountantId, ActualCostConfirmationDto dto);
        Task RollbackReceptionAsync(int proposalId, string reason);
        Task<int> SyncMissingVehiclesAsync();
    }
}
