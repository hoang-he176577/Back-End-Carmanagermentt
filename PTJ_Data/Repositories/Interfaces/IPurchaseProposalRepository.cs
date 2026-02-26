using Models.Models;

namespace Data.Repositories.Interfaces
{
    public interface IPurchaseProposalRepository
    {
        Task<PurchaseProposal?> GetByIdAsync(int id);
        Task<List<PurchaseProposal>> GetAllAsync();
        Task AddAsync(PurchaseProposal proposal);
        void Update(PurchaseProposal proposal);
        void Delete(PurchaseProposal proposal);
        Task SaveChangesAsync();
    }
}
