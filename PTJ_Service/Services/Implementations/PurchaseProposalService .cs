    using Data.Repositories.Interfaces;
    using Models.Models;
    using Service.Services.Interfaces;

using Models.DTO.PurchaseProposal;
<<<<<<< HEAD
using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
=======
using Models.Models;
using Service.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
>>>>>>> thaibd


    namespace Service.Services.Implementations
    {
<<<<<<< HEAD

        public class PurchaseProposalService : IPurchaseProposalService
        {
            private readonly IPurchaseProposalRepository _repository;

            public PurchaseProposalService(IPurchaseProposalRepository repository)
            {
                _repository = repository;
            }

            public async Task<List<PurchaseProposalListDto>> GetAllAsync()
=======
        private readonly IPurchaseProposalRepository _repository;
        private readonly CarManagerContext _context;

        public PurchaseProposalService(IPurchaseProposalRepository repository, CarManagerContext context)
        {
            _repository = repository;
            _context = context;
        }

        public async Task<List<PurchasePlanDto>> GetAllAsync()
        {
            var proposals = await _repository.GetAllAsync();
            return proposals
                .Select(p => new PurchasePlanDto
                {
                    ProposalId = p.Id,
                    Description = p.Description,
                    Status = p.Status,
                    CreatedDate = p.CreatedDate,
                    ProposedCost = p.ProposedCost,
                    ManagerName = p.Manager?.Name,
                    Priority = CalculatePriority(p),
                    BranchDetails = p.BulkPurchaseDetails.Select(d => new BranchPurchaseDetailDto
                    {
                        BranchId = d.BranchId,
                        BranchName = d.Branch?.Name,
                        ProposedQuantity = d.ProposedQuantity,
                        UnitPrice = d.UnitPrice,
                        Seats = d.Seats,
                        Manufacturer = d.Manufacturer,
                        BranchNotes = d.BranchNotes,
                        RequestedDate = p.CreatedDate
                    }).ToList()
                })
                .OrderBy(p => p.Status == "Pending" ? 1 :
                              p.Status == "Received_Pending_Payment" ? 2 :
                              p.Status == "Approved" ? 3 : 4) // 1. Trạng thái cần xử lý xếp lên đầu
                .ThenBy(p => p.Priority) // 2. Theo mức độ ưu tiên
                .ThenBy(p => p.CreatedDate) // 3. Theo ngày tạo (cũ nhất xử lý trước)
                .ThenByDescending(p => p.ProposedCost) // 4. Theo giá trị (chi phí cao hơn xếp trên)
                .ToList();
        }

                public async Task<PurchaseProposal?> GetByIdAsync(int id)
        {
            return await _repository.GetByIdAsync(id);
        }

        public async Task<object> CreateAsync(CreatePurchaseProposalDto dto, int proposerId, int branchId)
        {
            if (dto.Details == null || !dto.Details.Any())
                throw new Exception("Đề xuất phải có ít nhất 1 chi tiết cấu hình xe.");

            if (string.IsNullOrWhiteSpace(dto.Description))
                throw new Exception("Mô tả/lý do đề xuất không được để trống.");

            // KIỂM TRA BẢO MẬT VÀ TÍNH ĐỒNG NHẤT CỦA TÀI KHOẢN
            var proposer = await _context.Users.FindAsync(proposerId);
            if (proposer == null)
                throw new Exception("Tài khoản người yêu cầu không tồn tại trong hệ thống.");
            if (proposer.BranchId.HasValue && proposer.BranchId.Value != branchId)
                throw new Exception("Lỗi bảo mật: Dữ liệu chi nhánh không khớp với thông tin tài khoản của bạn.");

            var branchExists = await _context.Branches.AnyAsync(b => b.Id == branchId);
            if (!branchExists)
                throw new Exception($"Chi nhánh của bạn (ID: {branchId}) không tồn tại trong hệ thống.");

            var proposal = new PurchaseProposal();
            proposal.InitCreate(dto.Description, proposerId);

            if (dto.Details != null)
            {
                foreach (var detail in dto.Details)
                {   
                    if (detail.Quantity <= 0) throw new Exception("Số lượng xe phải lớn hơn 0.");
                    if (detail.UnitPrice <= 0) throw new Exception("Đơn giá xe phải lớn hơn 0.");
                    if (detail.UnitPrice > 9999999999999M) throw new Exception("Đơn giá xe vượt mức tối đa cho phép của hệ thống.");
                    if (string.IsNullOrWhiteSpace(detail.Manufacturer)) throw new Exception("Nhãn hiệu xe không được để trống.");
                    if (detail.Seats.HasValue && detail.Seats.Value <= 0) throw new Exception("Số chỗ ngồi không hợp lệ.");

                    var bulkDetail = new BulkPurchaseDetail();
                    bulkDetail.InitCreate(
                        branchId, // Lấy chi nhánh trực tiếp từ token truyền xuống, KHÔNG lấy từ detail DTO nữa
                        detail.Quantity,
                        detail.UnitPrice,
                        detail.Notes ?? detail.Description);

                    bulkDetail.Seats = detail.Seats;
                    bulkDetail.Manufacturer = detail.Manufacturer;

                    proposal.AddDetail(bulkDetail);
                }
            }


            await _repository.AddAsync(proposal);
            await _repository.SaveChangesAsync();

            return new
>>>>>>> thaibd
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

        // ==============================
        // LUỒNG TỰ ĐỘNG TẠO XE - KẾ TOÁN XÁC NHẬN
        // ==============================
        public async Task ConfirmPaymentAsync(int proposalId, int accountantId)
        {
            var proposal = await _context.PurchaseProposals
                .Include(p => p.BulkPurchaseDetails)
                .FirstOrDefaultAsync(p => p.Id == proposalId);

            if (proposal == null) throw new Exception("Không tìm thấy đề xuất");

            var receptionRecords = await _context.VehicleReceptionRecords
                .Where(r => r.PurchaseProposalId == proposalId && r.Status == VehicleReceptionRecord.ReceivedPendingPaymentStatus)
                .ToListAsync();
                
            if (!receptionRecords.Any())
                throw new Exception("Không tìm thấy bản ghi đối chiếu nào đang chờ thanh toán cho đề xuất này.");

            // 1. KIỂM TRA TRÙNG LẶP TRƯỚC KHI THỰC HIỆN
            foreach (var record in receptionRecords)
            {
                if (string.IsNullOrWhiteSpace(record.LicensePlate))
                    throw new Exception($"Bản ghi đối chiếu #{record.Id} bị thiếu biển số xe.");

                // KIỂM TRA CHẶT CHẼ TRƯỚC KHI INSERT VÀO DB (UNIQUE CONSTRAINT)
                bool isExist = await _context.Vehicles.AnyAsync(v => v.LicensePlate == record.LicensePlate);
                if (isExist)
                {
                    throw new Exception($"Lỗi: Biển số {record.LicensePlate} đã tồn tại trong kho. Vui lòng hoàn tác đối chiếu!");
                }
            }

            // 2. NẾU MỌI THỨ HỢP LỆ, TIẾN HÀNH DUYỆT THANH TOÁN
            proposal.MarkAsCompleted();
            proposal.ApproveByChiefAccountant(accountantId);

            foreach (var record in receptionRecords)
            {
                var detail = proposal.BulkPurchaseDetails.FirstOrDefault(d => d.BranchId == record.BranchId);

                var newVehicle = new Vehicle
                {
                    LicensePlate = record.LicensePlate,
                    CurrentBranchId = record.BranchId,
                    Status = "Active",
                    PurchaseDate = DateOnly.FromDateTime(DateTime.Now),
                    OriginalCost = detail?.UnitPrice ?? 0,
                    CurrentValue = detail?.UnitPrice ?? 0,
                    Mileage = 0,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _context.Vehicles.Add(newVehicle);
                
                record.Complete();
            }

            await _context.SaveChangesAsync();
        }

        public async Task RollbackReceptionAsync(int proposalId, string reason)
        {
            var proposal = await _context.PurchaseProposals.FindAsync(proposalId)
                ?? throw new Exception("Không tìm thấy đề xuất");

            var receptionRecords = await _context.VehicleReceptionRecords
                .Where(r => r.PurchaseProposalId == proposalId && r.Status != VehicleReceptionRecord.RejectedStatus)
                .ToListAsync();

            proposal.RevertToApproved(reason);
            _context.VehicleReceptionRecords.RemoveRange(receptionRecords);
            await _context.SaveChangesAsync();
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
<<<<<<< HEAD
        public async Task ConfirmReceiptAsync(int proposalId, string notes)
        {
            var proposal = await _repository.GetByIdAsync(proposalId)
                           ?? throw new Exception("Không tìm thấy đề xuất");

            proposal.ConfirmReceipt(notes);

            _repository.Update(proposal);
            await _repository.SaveChangesAsync();
        }
    }
=======

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
                .Where(p => (p.Status == "Approved" || p.Status == VehicleReceptionRecord.ReceivedPendingPaymentStatus) && p.DeletedAt == null)
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
                            Seats = d.Seats,
                            Manufacturer = d.Manufacturer,
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
                        Seats = d.Seats,
                        Manufacturer = d.Manufacturer,
                        BranchNotes = d.BranchNotes,
                        RequestedDate = p.CreatedDate
                    })
                    .ToList()
            })
            .OrderBy(p => p.Priority) // 1. Ưu tiên cao nhất
            .ThenBy(p => p.CreatedDate) // 2. Theo ngày tạo (cũ nhất ưu tiên trước)
            .ThenByDescending(p => p.ProposedCost) // 3. Theo giá trị (cao xếp trước)
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
>>>>>>> thaibd
    }
