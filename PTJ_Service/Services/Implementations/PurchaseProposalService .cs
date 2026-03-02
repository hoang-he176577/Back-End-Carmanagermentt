using Azure.Core;
using Data.Repositories.Interfaces;
using Models.DTO.PurchaseProposal;
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

        public async Task<List<PurchaseProposalListDto>> GetAllAsync()
        {
            var entities = await _repository.GetAllAsync();

            return entities.Select(x => new PurchaseProposalListDto
            {
                Id = x.Id,
                Description = x.Description,
                Status = x.Status,
                CreatedDate = x.CreatedDate,
                ProposedCost = x.ProposedCost,
                ManagerName = x.Manager != null ? x.Manager.Name : null
            }).ToList();
        }

        public async Task<PurchaseProposal?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<object> CreateAsync(CreatePurchaseProposalDto dto)
        {
            if (dto.Details == null || !dto.Details.Any())
                throw new Exception("At least one detail is required");

            var proposal = new PurchaseProposal();

            proposal.InitCreate(dto.Description);

            foreach (var item in dto.Details)
            {
                var detail = new BulkPurchaseDetail();
                detail.InitCreate(item.BranchId, item.Quantity, item.UnitPrice , item.Notes);

                proposal.AddDetail(detail);
            }

            await _repository.AddAsync(proposal);
            await _repository.SaveChangesAsync();

            return new
            {
                proposal.Id,
                proposal.Status
            };
        }

        public async Task ApproveByManagerAsync(int proposalId, int managerId)
        {
            var proposal = await _repository.GetByIdAsync(proposalId)
                ?? throw new Exception("Proposal not found");

            proposal.ApproveByManager(managerId);

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
