using Models.DTO.PurchaseProposal;
using Models.Models;
using System;


namespace Service.Services.Interfaces
{
    public interface IPurchaseProposalService
    {
        Task<List<PurchaseProposalListDto>> GetAllAsync();
        Task<PurchaseProposal?> GetByIdAsync(int id);
        Task<object> CreateAsync(CreatePurchaseProposalDto dto, int proposerId, int branchId);
        Task ApproveByManagerAsync(int proposalId, int managerId);
        Task RejectAsync(int proposalId, string reason);
        Task DeleteAsync(int proposalId);

        Task<List<PurchaseProposal>> GetPendingForManagerAsync();

        Task<List<PurchaseProposalDto>> GetApprovedByBranchAsync(int branchId);

    }
}
