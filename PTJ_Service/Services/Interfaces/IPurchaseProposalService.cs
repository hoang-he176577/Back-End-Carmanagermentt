using Models.Models;
using System;


namespace Service.Services.Interfaces
{
    public interface IPurchaseProposalService
    {
        Task<List<PurchaseProposal>> GetAllAsync();
        Task<PurchaseProposal?> GetByIdAsync(int id);
        Task<PurchaseProposal> CreateAsync(string description);
        Task ApproveByManagerAsync(int proposalId, int managerId);
        Task ApproveByChiefAccountantAsync(int proposalId, int accountantId);
        Task RejectAsync(int proposalId, string reason);
        Task DeleteAsync(int proposalId);
    }
}
