    using Data.Repositories.Interfaces;
    using Models.Models;
    using Service.Services.Interfaces;

using Models.DTO.PurchaseProposal;

using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;



    namespace Service.Services.Implementations
    {


        public class PurchaseProposalService : IPurchaseProposalService
        {
           

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
                    CompletionDeadline = p.CompletionDeadline,
                    ProposedCost = p.ProposedCost,
                    ManagerName = p.Manager?.Name,
                    Priority = CalculatePriority(p),
                    BranchDetails = p.BulkPurchaseDetails.Select(d => new BranchPurchaseDetailDto
                    {
                        ProposerBranchName = p.Proposer?.Branch?.Name,
                        BranchId = d.BranchId ?? 0,
                        BranchName = d.Branch?.Name,
                        ProposedQuantity = d.ProposedQuantity ?? 0,
                        UnitPrice = d.UnitPrice ?? 0,
                        Seats = d.Seats,
                        Manufacturer = d.Manufacturer,
                        Version = d.Version,
                        FuelNorm = d.FuelNorm,
                        RegistrationTax = d.RegistrationTax,
                        RoadMaintenanceFee = d.RoadMaintenanceFee,
                        LicensePlateFee = d.LicensePlateFee,
                        InsuranceFee = d.InsuranceFee,
                        HasCamera158 = d.HasCamera158 ?? false,
                        HasGsht = d.HasGsht ?? false,
                        AcquisitionMethod = d.AcquisitionMethod,
                        BranchNotes = d.BranchNotes,
                        RequestedDate = p.CreatedDate,
                        CompletionDeadline = p.CompletionDeadline
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
                throw new ArgumentException("Đề xuất phải có ít nhất 1 chi tiết cấu hình xe.");

            if (string.IsNullOrWhiteSpace(dto.Description))
                throw new ArgumentException("Mô tả/lý do đề xuất không được để trống.");

            if (!dto.CompletionDeadline.HasValue)
                throw new ArgumentException("Vui lòng chọn Hạn hoàn thành cho đề xuất.");

            if (dto.CompletionDeadline.Value.Date < DateTime.Now.Date)
                throw new ArgumentException("Hạn hoàn thành không được là một ngày trong quá khứ.");

            var proposer = await _context.Users.FindAsync(proposerId);
            if (proposer == null)
                throw new ArgumentException("Tài khoản người yêu cầu không tồn tại trong hệ thống.");
            if (proposer.BranchId.HasValue && proposer.BranchId.Value != branchId)
                throw new ArgumentException("Lỗi bảo mật: Dữ liệu chi nhánh không khớp với thông tin tài khoản của bạn.");

            var branchExists = await _context.Branches.AnyAsync(b => b.Id == branchId);
            if (!branchExists)
                throw new ArgumentException($"Chi nhánh của bạn (ID: {branchId}) không tồn tại trong hệ thống.");

            var proposal = new PurchaseProposal();
            proposal.InitCreate(dto.Description, proposerId, dto.CompletionDeadline);

            if (dto.Details != null)
            {
                foreach (var detail in dto.Details)
                {
                    if (detail.Quantity <= 0) throw new ArgumentException("Số lượng xe phải lớn hơn 0.");
                    if (detail.UnitPrice <= 0) throw new ArgumentException("Đơn giá xe phải lớn hơn 0.");
                    if (string.IsNullOrWhiteSpace(detail.Manufacturer)) throw new ArgumentException("Nhãn hiệu xe không được để trống.");

                    // Legal Validation (Nghị định 158/2024)
                    if ((detail.Seats.HasValue && detail.Seats.Value >= 8)) // Điều kiện xe kinh doanh/chở người
                    {
                        if (detail.HasCamera158 != true || detail.HasGsht != true)
                        {
                            throw new ArgumentException("Theo Nghị định 158/2024, phương tiện kinh doanh/chở người trên 8 chỗ bắt buộc phải lắp đặt Camera và thiết bị GSHT.");
                        }
                    }

                    var bulkDetail = new BulkPurchaseDetail();
                    bulkDetail.InitCreate(
                        branchId,
                        detail.Quantity,
                        detail.UnitPrice,
                        detail.Notes ?? detail.Description);

                    // Map TCO & Legal fields
                    bulkDetail.Seats = detail.Seats;
                    bulkDetail.Manufacturer = detail.Manufacturer;
                    bulkDetail.Version = detail.Version;
                    bulkDetail.AcquisitionMethod = detail.AcquisitionMethod;
                    bulkDetail.RegistrationTax = detail.RegistrationTax;
                    bulkDetail.RoadMaintenanceFee = detail.RoadMaintenanceFee;
                    bulkDetail.LicensePlateFee = detail.LicensePlateFee;
                    bulkDetail.InsuranceFee = detail.InsuranceFee;
                    bulkDetail.HasCamera158 = detail.HasCamera158;
                    bulkDetail.HasGsht = detail.HasGsht;

                    proposal.AddDetail(bulkDetail);
                }
            }
            
            await _repository.AddAsync(proposal);
            await _repository.SaveChangesAsync();

            return proposal;
        }

        public async Task<object> UpdateAsync(int proposalId, UpdatePurchaseProposalDto dto, int proposerId, int branchId)
        {
            var proposal = await _context.PurchaseProposals
                .Include(p => p.BulkPurchaseDetails)
                .FirstOrDefaultAsync(p => p.Id == proposalId);

            if (proposal == null) throw new ArgumentException("Không tìm thấy đề xuất.");
            if (proposal.ProposerId != proposerId) throw new ArgumentException("Bạn không có quyền sửa đề xuất này.");
            if (proposal.Status != "Pending") throw new ArgumentException("Chỉ có thể sửa đề xuất khi đang ở trạng thái chờ duyệt.");

            if (dto.Details == null || !dto.Details.Any())
                throw new ArgumentException("Đề xuất phải có ít nhất 1 chi tiết cấu hình xe.");
            if (string.IsNullOrWhiteSpace(dto.Description))
                throw new ArgumentException("Mô tả/lý do đề xuất không được để trống.");

            var branchExists = await _context.Branches.AnyAsync(b => b.Id == branchId);
            if (!branchExists) throw new ArgumentException($"Chi nhánh không tồn tại.");

            proposal.Description = dto.Description;
            proposal.CompletionDeadline = dto.CompletionDeadline;
            proposal.UpdatedAt = DateTime.Now;

            _context.BulkPurchaseDetails.RemoveRange(proposal.BulkPurchaseDetails);
            
            foreach (var detail in dto.Details)
            {
                if (detail.Quantity <= 0) throw new ArgumentException("Số lượng xe phải lớn hơn 0.");
                if (detail.UnitPrice <= 0) throw new ArgumentException("Đơn giá xe phải lớn hơn 0.");
                if (string.IsNullOrWhiteSpace(detail.Manufacturer)) throw new ArgumentException("Nhãn hiệu xe không được để trống.");

                if ((detail.Seats.HasValue && detail.Seats.Value >= 8))
                {
                    if (detail.HasCamera158 != true || detail.HasGsht != true)
                    {
                        throw new ArgumentException("Theo Nghị định 158/2024, phương tiện kinh doanh/chở người trên 8 chỗ bắt buộc phải lắp đặt Camera và thiết bị GSHT.");
                    }
                }

                var bulkDetail = new BulkPurchaseDetail();
                bulkDetail.InitCreate(branchId, detail.Quantity, detail.UnitPrice, detail.Notes ?? detail.Description);
                bulkDetail.Seats = detail.Seats;
                bulkDetail.Manufacturer = detail.Manufacturer;
                bulkDetail.AcquisitionMethod = detail.AcquisitionMethod;
                bulkDetail.Version = detail.Version;
                bulkDetail.RegistrationTax = detail.RegistrationTax;
                bulkDetail.RoadMaintenanceFee = detail.RoadMaintenanceFee;
                bulkDetail.LicensePlateFee = detail.LicensePlateFee;
                bulkDetail.InsuranceFee = detail.InsuranceFee;
                bulkDetail.HasCamera158 = detail.HasCamera158;
                bulkDetail.HasGsht = detail.HasGsht;

                bulkDetail.PurchaseProposalId = proposal.Id;
                _context.BulkPurchaseDetails.Add(bulkDetail);
            }

            proposal.ProposedCost = dto.Details.Sum(d => d.Quantity * (d.UnitPrice + (d.RegistrationTax ?? 0) + (d.RoadMaintenanceFee ?? 0) + (d.LicensePlateFee ?? 0) + (d.InsuranceFee ?? 0)));

            await _context.SaveChangesAsync();
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
        public async Task ConfirmPaymentAsync(int proposalId, int accountantId, ActualCostConfirmationDto dto)
        {
            var proposal = await _context.PurchaseProposals
                .Include(p => p.BulkPurchaseDetails)
                .FirstOrDefaultAsync(p => p.Id == proposalId);

            if (proposal == null) throw new Exception("Không tìm thấy đề xuất");

            var receptionRecords = await _context.VehicleReceptionRecords
                .Where(r => r.PurchaseProposalId == proposalId && 
                       (r.Status == VehicleReceptionRecord.ReceivedPendingPaymentStatus || r.Status == VehicleReceptionRecord.PendingStatus))
                .ToListAsync();
                
            if (!receptionRecords.Any())
                throw new Exception("Không tìm thấy bản ghi đối chiếu nào đang chờ thanh toán cho đề xuất này.");

            // 2. CẬP NHẬT TRẠNG THÁI VÀ CHI PHÍ (CƠ CHẾ MỚI: KIỂM TRA HOÀT TẤT TOÀN BỘ CHIẾC XE)
            int totalProposed = proposal.BulkPurchaseDetails.Sum(d => d.ProposedQuantity ?? 0);
            int totalReceivedAndPaid = await _context.VehicleReceptionRecords
                .CountAsync(r => r.PurchaseProposalId == proposalId && r.Status == VehicleReceptionRecord.CompletedStatus) 
                + receptionRecords.Count; // Cộng thêm những bản ghi sắp được hoàn thành ở bước sau

            if (totalReceivedAndPaid >= totalProposed)
            {
                proposal.MarkAsCompleted();
            }
            else
            {
                // Nếu chưa đủ xe, trả về trạng thái Approved để Operator có thể tiếp nhận tiếp các xe còn lại
                proposal.ApproveByChiefAccountant(accountantId);
            }

            proposal.ApproveByChiefAccountant(accountantId);
            proposal.ActualCost = (proposal.ActualCost ?? 0) + dto.ActualCost;

            // CẬP NHẬT CHI PHÍ CHI TIẾT NẾU CÓ
            if (dto.DetailUpdates != null && dto.DetailUpdates.Any())
            {
                foreach (var update in dto.DetailUpdates)
                {
                    var detail = proposal.BulkPurchaseDetails.FirstOrDefault(d => d.Id == update.DetailId);
                    if (detail != null)
                    {
                        // Lưu lại giá trị sau cùng được kế toán duyệt
                        detail.UnitPrice = update.UnitPrice;
                        detail.RegistrationTax = update.RegistrationTax;
                        detail.RoadMaintenanceFee = update.RoadMaintenanceFee;
                        detail.LicensePlateFee = update.LicensePlateFee;
                        detail.InsuranceFee = update.InsuranceFee;
                        detail.ReceivedDate = DateTime.Now;
                        
                        // Nếu đã nhận đủ số lượng của dòng xe này thì mới mark Detail là Completed
                        int receivedForThisDetail = await _context.VehicleReceptionRecords
                            .CountAsync(r => r.PurchaseProposalId == proposalId && r.BranchId == detail.BranchId && r.Version == detail.Version && r.Status == VehicleReceptionRecord.CompletedStatus)
                            + receptionRecords.Count(r => r.BranchId == detail.BranchId && r.Version == detail.Version);

                        if (receivedForThisDetail >= (detail.ProposedQuantity ?? 0))
                        {
                            detail.Status = "Completed";
                        }
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(dto.AccountantNote))
            {
                proposal.Description += $"\n[Ghi chú Kế toán Thực chi]: {dto.AccountantNote}";
            }

            foreach (var record in receptionRecords)
            {
                // Chỉ cần đánh dấu bản ghi đối chiếu là đã hoàn thành
                record.Complete();

                // ĐỒNG BỘ DỮ LIỆU SANG KHO TÀI SẢN (Cập nhật nguyên giá thực tế)
                var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.LicensePlate == record.LicensePlate);
                
                // Tìm detail tương ứng để lấy cấu hình xe
                var detail = proposal.BulkPurchaseDetails.FirstOrDefault(d => 
                    d.BranchId == record.BranchId && d.Version == record.Version);

                if (vehicle == null && detail != null)
                {
                    // TRƯỜNG HỢP CỰC KỲ QUAN TRỌNG: Nếu xe chưa được tạo lúc đối chiếu (do lỗi hoặc dữ liệu cũ), 
                    // thì phải tạo ngay lúc thanh toán để đảm bảo tài sản không bị mất.
                    
                    // Tìm hoặc tạo model
                    var model = await _context.VehicleModels
                        .FirstOrDefaultAsync(m => m.Manufacturer == detail.Manufacturer && m.ModelName == record.Version);
                    if (model == null)
                    {
                        model = new VehicleModel { 
                            Manufacturer = detail.Manufacturer ?? "N/A", 
                            ModelName = record.Version ?? "N/A", 
                            CreatedAt = DateTime.Now, 
                            UpdatedAt = DateTime.Now 
                        };
                        _context.VehicleModels.Add(model);
                        await _context.SaveChangesAsync();
                    }

                    vehicle = new Vehicle
                    {
                        LicensePlate = record.LicensePlate,
                        Vin = record.Vin,
                        ChassisNumber = record.ChassisNumber,
                        EngineNumber = record.EngineNumber,
                        TelematicsImei = record.TelematicsImei,
                        CurrentBranchId = record.BranchId,
                        ModelId = model.Id,
                        Status = "Active",
                        PurchaseDate = DateOnly.FromDateTime(DateTime.Now),
                        YearManufacture = record.YearManufacture ?? DateTime.Now.Year,
                        Mileage = record.Mileage ?? 0,
                        CreatedAt = DateTime.Now
                    };
                    _context.Vehicles.Add(vehicle);
                }

                if (vehicle != null && detail != null)
                {
                    // Cập nhật nguyên giá thực tế dựa trên số liệu Kế toán vừa chốt
                    decimal totalCost = (detail.UnitPrice ?? 0) + 
                                       (detail.RegistrationTax ?? 0) + 
                                       (detail.LicensePlateFee ?? 0) + 
                                       (detail.RoadMaintenanceFee ?? 0) + 
                                       (detail.InsuranceFee ?? 0);
                    
                    vehicle.OriginalCost = totalCost;
                    vehicle.CurrentValue = totalCost;
                    
                    // Đồng bộ lại các trường thông tin khác
                    vehicle.RegistrationExpirationDate = record.RegistrationExpirationDate;
                    vehicle.InsuranceExpirationDate = record.InsuranceExpirationDate;
                    vehicle.BadgeExpirationDate = record.BadgeExpirationDate;
                    vehicle.FuelNorm = record.FuelNorm;
                    vehicle.YearManufacture = record.YearManufacture;
                    vehicle.Mileage = record.Mileage;
                    vehicle.Status = "Active";
                    vehicle.UpdatedAt = DateTime.Now;
                }
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
            // Lọc nhữn cái có Status là Pending và chưa bị xóa
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
                    CompletionDeadline = p.CompletionDeadline,
                    
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
    


        /// <summary>
        /// Lấy danh sách kế hoạch mua (approved proposals)
        /// - Nếu branchId = null: lấy tất cả (cho Manager)
        /// - Nếu branchId > 0: lấy chỉ kế hoạch của chi nhánh đó (cho Operator)
        /// Sắp xếp theo ưu tiên (Priority) 
        /// </summary>
        public async Task<List<PurchasePlanDto>> GetPurchasePlanAsync(int? branchId = null)
        {
            var proposals = await _repository.GetAllAsync();

            var filtered = proposals.Where(p => 
                p.DeletedAt == null && (
                p.Status == PurchaseProposal.ManagerApprovedStatus ||
                p.Status == PurchaseProposal.ApprovedStatus ||
                p.Status == "Received_Pending_Payment" ||
                p.Status == "Received_Pending_Approval" ||
                p.Status == "Completed"
            )).ToList();

            if (branchId.HasValue && branchId.Value > 0)
            {
                filtered = filtered.Where(p => p.BulkPurchaseDetails.Any(d => d.BranchId == branchId.Value)).ToList();
            }

            return filtered.Select(p => new PurchasePlanDto
            {
                ProposalId = p.Id,
                Description = p.Description,
                Status = p.Status,
                CreatedDate = p.CreatedDate,
                ApprovedDate = p.ApprovedDate,
                CompletionDeadline = p.CompletionDeadline,
                ProposedCost = p.ProposedCost,
                ManagerName = p.Manager?.Name,
                Priority = CalculatePriority(p),

                Receptions = p.VehicleReceptionRecords?
                    .Where(r => r.Status != VehicleReceptionRecord.RejectedStatus)
                    .Select(r => new VehicleReceptionRecordDto
                    {
                        Id = r.Id,
                        ProposalId = r.PurchaseProposalId,
                        BranchId = r.BranchId,
                        BranchName = r.Branch?.Name,
                        LicensePlate = r.LicensePlate,
                        Vin = r.Vin,
                        TelematicsImei = r.TelematicsImei,
                        Version = r.Version,
                        ChassisNumber = r.ChassisNumber,
                        EngineNumber = r.EngineNumber,
                        ReceivedDate = r.ReceivedDate,
                        RegistrationExpirationDate = r.RegistrationExpirationDate,
                        InsuranceExpirationDate = r.InsuranceExpirationDate,
                        BadgeType = r.BadgeType,
                        BadgeExpirationDate = r.BadgeExpirationDate,
                        FuelNorm = r.FuelNorm,
                        ReceiptImageUrl = r.ReceiptImageUrl,
                        Status = r.Status,
                        Notes = r.Notes
                    }).ToList() ?? new List<VehicleReceptionRecordDto>(),

                BranchDetails = p.BulkPurchaseDetails.Select(d => new BranchPurchaseDetailDto
                {
                    Id = d.Id,
                    ProposerBranchName = p.Proposer?.Branch?.Name,
                    BranchId = d.BranchId ?? 0,
                    BranchName = d.Branch?.Name,
                    ProposedQuantity = d.ProposedQuantity ?? 0,
                    UnitPrice = d.UnitPrice ?? 0,
                    Seats = d.Seats,
                    Manufacturer = d.Manufacturer,
                    Version = d.Version,
                    BranchNotes = d.BranchNotes,
                    RequestedDate = p.CreatedDate,
                    CompletionDeadline = p.CompletionDeadline,
                    ReceivedQuantity = p.VehicleReceptionRecords?.Count(r => r.BranchId == d.BranchId && r.Status != VehicleReceptionRecord.RejectedStatus) ?? 0,
                    RegistrationTax = d.RegistrationTax,
                    RoadMaintenanceFee = d.RoadMaintenanceFee,
                    LicensePlateFee = d.LicensePlateFee,
                    InsuranceFee = d.InsuranceFee,
                    HasCamera158 = d.HasCamera158 ?? false,
                    HasGsht = d.HasGsht ?? false,
                    AcquisitionMethod = d.AcquisitionMethod
                }).ToList()
            })
            .OrderBy(p => p.Priority)
            .ThenBy(p => p.CreatedDate)
            .ToList();
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
        public async Task<int> SyncMissingVehiclesAsync()
        {
            // Lấy tất cả bản ghi đối chiếu đã hoàn thành (đã thanh toán) nhưng có thể chưa có bản ghi xe
            var completedRecords = await _context.VehicleReceptionRecords
                .Include(r => r.PurchaseProposal)
                .ThenInclude(p => p.BulkPurchaseDetails)
                .Where(r => r.Status == VehicleReceptionRecord.CompletedStatus)
                .ToListAsync();

            int syncCount = 0;
            foreach (var record in completedRecords)
                {
                // Kiểm tra xem biển số đã tồn tại trong kho xe chưa
                var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.LicensePlate == record.LicensePlate);
                
                var proposal = record.PurchaseProposal;
                var detail = proposal?.BulkPurchaseDetails.FirstOrDefault(d => 
                    d.BranchId == record.BranchId && d.Version == record.Version);

                bool isNew = false;
                if (vehicle == null && detail != null)
                {
                    // TẠO MỚI NẾU THIẾU
                    var model = await _context.VehicleModels
                        .FirstOrDefaultAsync(m => m.Manufacturer == detail.Manufacturer && m.ModelName == record.Version);
                    
                    if (model == null)
                    {
                        model = new VehicleModel { 
                            Manufacturer = detail.Manufacturer ?? "N/A", 
                            ModelName = record.Version ?? "N/A", 
                            CreatedAt = DateTime.Now, 
                            UpdatedAt = DateTime.Now 
                        };
                        _context.VehicleModels.Add(model);
                        await _context.SaveChangesAsync();
                    }

                    vehicle = new Vehicle
                    {
                        LicensePlate = record.LicensePlate,
                        Vin = record.Vin,
                        ChassisNumber = record.ChassisNumber,
                        EngineNumber = record.EngineNumber,
                        TelematicsImei = record.TelematicsImei,
                        CurrentBranchId = record.BranchId,
                        ModelId = model.Id,
                        Status = "Active",
                        PurchaseDate = record.ReceivedDate,
                        YearManufacture = record.YearManufacture ?? DateTime.Now.Year,
                        Mileage = record.Mileage ?? 0,
                        CreatedAt = DateTime.Now
                    };
                    _context.Vehicles.Add(vehicle);
                    isNew = true;
                }

                if (vehicle != null && detail != null)
                {
                    // ĐỒNG BỘ LẠI GIÁ TRỊ (Kể cả xe đã có nhưng có thể sai lệch giá)
                    decimal totalCost = (detail.UnitPrice ?? 0) + 
                                       (detail.RegistrationTax ?? 0) + 
                                       (detail.LicensePlateFee ?? 0) + 
                                       (detail.RoadMaintenanceFee ?? 0) + 
                                       (detail.InsuranceFee ?? 0);
                    
                    if (vehicle.OriginalCost != totalCost || isNew)
                    {
                        vehicle.OriginalCost = totalCost;
                        vehicle.CurrentValue = totalCost;
                        vehicle.RegistrationExpirationDate = record.RegistrationExpirationDate;
                        vehicle.InsuranceExpirationDate = record.InsuranceExpirationDate;
                        vehicle.BadgeExpirationDate = record.BadgeExpirationDate;
                        vehicle.FuelNorm = record.FuelNorm;
                        vehicle.UpdatedAt = DateTime.Now;
                        syncCount++;
                    }
                }
            }

            await _context.SaveChangesAsync();
            return syncCount;
        }
    }
}
