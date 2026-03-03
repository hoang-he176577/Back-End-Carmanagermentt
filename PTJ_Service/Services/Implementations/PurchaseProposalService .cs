    using Data.Repositories.Interfaces;
    using Models.Models;
    using Service.Services.Interfaces;

using Models.DTO.PurchaseProposal;
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
                var proposals = await _repository.GetAllAsync();
                return proposals
                    .Where(p => p.DeletedAt == null)
                    .Select(p => new PurchaseProposalListDto
                    {
                        Id = p.Id,
                        Description = p.Description,
                        Status = p.Status,
                        CreatedDate = p.CreatedDate,
                        ProposedCost = p.ProposedCost,
                        ManagerName = p.Manager?.Name
                    })
                    .ToList();
            }

            public async Task<PurchaseProposal?> GetByIdAsync(int id)
            {
                return await _repository.GetByIdAsync(id);
            }

            public async Task<object> CreateAsync(CreatePurchaseProposalDto dto)
            {
                var proposal = new PurchaseProposal();
                proposal.InitCreate(dto.Description);

                if (dto.Details != null)
                {
                    foreach (var detail in dto.Details)
                    {
                        var bulkDetail = new BulkPurchaseDetail();
                        bulkDetail.InitCreate(detail.BranchId, detail.Quantity, detail.UnitPrice, detail.Notes);
                        proposal.AddDetail(bulkDetail);
                    }
                }

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

        public async Task<List<PurchaseProposal>> GetPendingForManagerAsync()
        {
            var all = await _repository.GetAllAsync();
            // Lọc những cái có Status là Pending và chưa bị xóa
            return all.Where(x => x.Status == "Pending" && x.DeletedAt == null).ToList();
        }
        public async Task<List<PurchaseProposalDto>> GetApprovedByBranchAsync(int branchId)
        {
            var proposals = await _repository.GetAllAsync();

            return proposals
                .Where(p => p.Status == "Approved" &&
                            p.BulkPurchaseDetails.Any(d => d.BranchId == branchId)) // Lọc theo chi nhánh
                .Select(p => new PurchaseProposalDto
                {
                    Id = p.Id,
                    
                    Description = p.Description ?? "",
                    Status = p.Status ?? "Approved",
                    ProposedCost = p.ProposedCost,
                    CreatedAt = p.CreatedAt,
                    
                    BranchNote = p.BulkPurchaseDetails
                                  .FirstOrDefault(d => d.BranchId == branchId)?.BranchNotes
                })
                .ToList();
        }
        public async Task ConfirmReceiptAsync(int proposalId, string notes)
        {
            var proposal = await _repository.GetByIdAsync(proposalId)
                           ?? throw new Exception("Không tìm thấy đề xuất");

            proposal.ConfirmReceipt(notes);

            _repository.Update(proposal);
            await _repository.SaveChangesAsync();
        }
    }
    }
