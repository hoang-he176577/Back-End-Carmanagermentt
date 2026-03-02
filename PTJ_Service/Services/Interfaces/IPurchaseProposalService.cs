using Models.DTO.PurchaseProposal;
using Models.Models;


namespace Service.Services.Interfaces
{
    public interface IPurchaseProposalService
    {
        Task<List<PurchaseProposalListDto>> GetAllAsync();
        Task<PurchaseProposal?> GetByIdAsync(int id);
        Task<object> CreateAsync(CreatePurchaseProposalDto dto);
        Task ApproveByManagerAsync(int proposalId, int managerId);
        Task RejectAsync(int proposalId, string reason);
        Task DeleteAsync(int proposalId);
    }
}
