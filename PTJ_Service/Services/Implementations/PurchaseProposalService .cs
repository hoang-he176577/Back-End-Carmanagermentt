using Data.Repositories.Interfaces;
using Models.Models;
using Service.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.Implementations
{

    public class PurchaseProposalService : IPurchaseProposalService
    {
        private readonly IPurchaseProposalRepository _repository;

        public PurchaseProposalService(IPurchaseProposalRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<PurchaseProposal>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }

        public async Task<PurchaseProposal?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<PurchaseProposal> CreateAsync( string description)
        {
            var proposal = new PurchaseProposal();
            proposal.InitCreate(description);

            await _repository.AddAsync(proposal);
            await _repository.SaveChangesAsync();

            return proposal;
        }

        public async Task ApproveByManagerAsync(int proposalId, int managerId)
        {
            var proposal = await _repository.GetByIdAsync(proposalId)
                ?? throw new Exception("Proposal not found");

            proposal.ApproveByManager(managerId);

            _repository.Update(proposal);
            await _repository.SaveChangesAsync();
        }

        public async Task ApproveByChiefAccountantAsync(int proposalId, int accountantId)
        {
            var proposal = await _repository.GetByIdAsync(proposalId)
                ?? throw new Exception("Proposal not found");

            proposal.ApproveByChiefAccountant(accountantId);

            _repository.Update(proposal);
            await _repository.SaveChangesAsync();
        }

        public async Task RejectAsync(int proposalId, string reason)
        {
            var proposal = await _repository.GetByIdAsync(proposalId)
                ?? throw new Exception("Proposal not found");

            proposal.Reject(reason);

            _repository.Update(proposal);
            await _repository.SaveChangesAsync();
        }

        public async Task DeleteAsync(int proposalId)
        {
            var proposal = await _repository.GetByIdAsync(proposalId)
                ?? throw new Exception("Proposal not found");

            proposal.SoftDelete();

            _repository.Update(proposal);
            await _repository.SaveChangesAsync();
        }
    }
}
