using Data.Repositories.Interfaces;
using Models.DTO.PurchaseProposal;
using Models.Models;
using Service.Services.Interfaces;

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
                    bulkDetail.InitCreate(
                        detail.BranchId,
                        detail.Quantity,
                        detail.UnitPrice,
                        detail.Notes ?? detail.Description);
                    proposal.AddDetail(bulkDetail);
                }
            }

            await _repository.AddAsync(proposal);
            await _repository.SaveChangesAsync();

            return new
            {
                proposal.Id,
                proposal.Status,
                proposal.ProposedCost
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

        public async Task<List<PurchaseProposal>> GetPendingForManagerAsync()
        {
            var all = await _repository.GetAllAsync();
            return all.Where(x => x.Status == "Pending" && x.DeletedAt == null).ToList();
        }

        public async Task<List<PurchaseProposalDto>> GetApprovedByBranchAsync(int branchId)
        {
            var proposals = await _repository.GetAllAsync();
            return proposals
                .Where(p => p.Status == "Approved" && p.BulkPurchaseDetails.Any(d => d.BranchId == branchId))
                .Select(p => new PurchaseProposalDto
                {
                    Id = p.Id,
                    Description = p.Description,
                    Status = p.Status ?? "Approved",
                    ProposedCost = p.ProposedCost,
                    CreatedAt = p.CreatedAt,
                    BranchNote = p.BulkPurchaseDetails.FirstOrDefault(d => d.BranchId == branchId)?.BranchNotes
                })
                .ToList();
        }

        /// <summary>
        /// Lấy danh sách kế hoạch mua (approved proposals)
        /// - Nếu branchId = null: lấy tất cả (cho Manager)
        /// - Nếu branchId > 0: lấy chỉ kế hoạch của chi nhánh đó (cho Operator)
        /// Sắp xếp theo ưu tiên (Priority) 
        /// </summary>
        public async Task<List<PurchasePlanDto>> GetPurchasePlanAsync(int? branchId = null)
        {
            var proposals = await _repository.GetAllAsync();

            var approved = proposals
                .Where(p => p.Status == "Approved" && p.DeletedAt == null)
                .ToList();

            // Nếu có branchId, lọc chỉ đề xuất có chi nhánh này
            if (branchId.HasValue && branchId.Value > 0)
            {
                approved = approved
                    .Where(p => p.BulkPurchaseDetails.Any(d => d.BranchId == branchId.Value))
                    .ToList();
            }

            // Map sang PurchasePlanDto
            var plans = approved.Select(p => new PurchasePlanDto
            {
                ProposalId = p.Id,
                Description = p.Description,
                Status = p.Status,
                CreatedDate = p.CreatedDate,
                ApprovedDate = p.ApprovedDate,
                ProposedCost = p.ProposedCost,
                ManagerName = p.Manager?.Name,
                Priority = CalculatePriority(p), // Tính priority dựa trên ngày + cost

                // Chi tiết theo chi nhánh
                BranchDetails = (branchId.HasValue && branchId.Value > 0)
                    ? p.BulkPurchaseDetails
                        .Where(d => d.BranchId == branchId.Value)
                        .Select(d => new BranchPurchaseDetailDto
                        {
                            BranchId = d.BranchId,
                            BranchName = d.Branch?.Name,
                            ProposedQuantity = d.ProposedQuantity,
                            UnitPrice = d.UnitPrice,
                            BranchNotes = d.BranchNotes,
                            RequestedDate = p.CreatedDate // Ngày yêu cầu
                        })
                        .ToList()
                    : p.BulkPurchaseDetails.Select(d => new BranchPurchaseDetailDto
                    {
                        BranchId = d.BranchId,
                        BranchName = d.Branch?.Name,
                        ProposedQuantity = d.ProposedQuantity,
                        UnitPrice = d.UnitPrice,
                        BranchNotes = d.BranchNotes,
                        RequestedDate = p.CreatedDate
                    })
                    .ToList()
            })
            .OrderBy(p => p.Priority) // Sắp xếp theo ưu tiên (1 = cao nhất)
            .ThenByDescending(p => p.ApprovedDate) // Sau đó theo ngày duyệt gần nhất
            .ToList();

            return plans;
        }

        /// <summary>
        /// Tính ưu tiên dựa trên ngày tạo và chi phí
        /// Priority 1: Cao (ngày cũ hơn hoặc chi phí cao)
        /// Priority 2: Trung bình
        /// Priority 3: Thấp (ngày gần hay chi phí thấp)
        /// </summary>
        private int CalculatePriority(PurchaseProposal proposal)
        {
            if (proposal.CreatedDate == null)
                return 3;

            var daysOld = (DateTime.Now.Date - proposal.CreatedDate.Value.ToDateTime(TimeOnly.MinValue)).Days;
            
            // Nếu hơn 30 ngày: Priority 1 (cao)
            if (daysOld > 30)
                return 1;
            
            // Nếu hơn 2 tuần: Priority 2 (trung bình)
            if (daysOld > 14)
                return 2;
            
            // Còn lại: Priority 3 (thấp)
            return 3;
        }
    }
}
