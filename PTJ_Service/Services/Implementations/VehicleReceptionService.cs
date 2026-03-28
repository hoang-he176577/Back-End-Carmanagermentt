using Microsoft.EntityFrameworkCore;
using Models.DTO.PurchaseProposal;
using Models.Models;
using Models.Exceptions;
using Service.Services.Interfaces;
using Data.Repositories.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Service.Services.Implementations
{
    public class VehicleReceptionService : IVehicleReceptionService
    {
        private readonly IVehicleReceptionRepository _repository;
        private readonly CarManagerContext _context;

        public VehicleReceptionService(IVehicleReceptionRepository repository, CarManagerContext context)
        {
            _repository = repository;
            _context = context;
        }

        public async Task<VehicleReceptionRecordDto?> GetByIdAsync(int id)
        {
            var record = await _repository.GetByIdAsync(id);
            if (record == null)
                return null;

            return MapToDto(record);
        }

        public async Task<List<VehicleReceptionRecordDto>> GetByProposalIdAsync(int proposalId)
        {
            var records = await _repository.GetByProposalIdAsync(proposalId);
            return records.Select(MapToDto).ToList();
        }

        public async Task<List<VehicleReceptionRecordDto>> GetByBranchIdAsync(int branchId)
        {
            var records = await _repository.GetByBranchIdAsync(branchId);
            return records.Select(MapToDto).ToList();
        }

        public async Task<List<VehicleReceptionRecordDto>> GetPendingByBranchAsync(int branchId)
        {
            var records = await _repository.GetPendingByBranchAsync(branchId);
            return records.Select(MapToDto).ToList();
        }

        public async Task<List<VehicleReceptionRecordDto>> GetAllAsync()
        {
            var records = await _repository.GetAllAsync();
            return records.Select(MapToDto).ToList();
        }

        public async Task<VehicleReceptionRecordDto> CreateAsync(CreateVehicleReceptionDto dto, int operatorId, DateOnly requestedDate)
        {
            // 1. KIỂM TRA TÍNH HỢP LỆ CỦA ĐỀ XUẤT VÀ CHI NHÁNH
            var proposal = await _context.PurchaseProposals
                .Include(p => p.BulkPurchaseDetails)
                .FirstOrDefaultAsync(p => p.Id == dto.PurchaseProposalId);
            if (proposal == null)
                throw new ArgumentException($"Đề xuất mua #{dto.PurchaseProposalId} không tồn tại trong hệ thống.");
            if (proposal.Status != PurchaseProposal.ApprovedStatus && 
                proposal.Status != PurchaseProposal.ManagerApprovedStatus &&
                proposal.Status != VehicleReceptionRecord.ReceivedPendingPaymentStatus)
            {
                throw new ArgumentException($"Đề xuất #{dto.PurchaseProposalId} chưa được duyệt hoặc đã xử lý xong (Trạng thái hiện tại: {proposal.Status}).");
            }

            var branchExists = await _context.Branches.AnyAsync(b => b.Id == dto.BranchId);
            if (!branchExists)
                throw new ArgumentException($"Chi nhánh #{dto.BranchId} không tồn tại.");

            // 2. KIỂM TRA SỐ LƯỢNG VÀ TRÙNG LẶP SỚM ĐỂ LẤY THÔNG TIN CẤU HÌNH
            var vehicleDetail = proposal.BulkPurchaseDetails.FirstOrDefault(d => d.BranchId == dto.BranchId);
            if (vehicleDetail == null) throw new ArgumentException("Đề xuất này không có xe nào dành cho chi nhánh của bạn.");
            int branchProposedQuantity = vehicleDetail.ProposedQuantity ?? 0;

            // 3. VALIDATION CẤM LỌT LỖI (FMS) - THEO CHUẨN CLEAN CODE
            var today = DateOnly.FromDateTime(DateTime.Now);

            // 3.1 Biển số xe (Regex + Chuẩn hóa)
            if (string.IsNullOrWhiteSpace(dto.LicensePlate)) 
                throw new InvalidLicensePlateException("Biển số xe không được để trống.");
            
            var cleanPlate = dto.LicensePlate.ToUpper().Replace(" ", "");
            if (!Regex.IsMatch(cleanPlate, @"^[0-9]{2}[A-Z]{1,2}-[0-9]{3,5}(\.[0-9]{2})?$"))
                throw new InvalidLicensePlateException();

            // 3.2 Số VIN (ISO Standard)
            if (string.IsNullOrWhiteSpace(dto.Vin)) 
                throw new InvalidVINLengthException("Số VIN không được để trống.");
            if (dto.Vin.Length != 17)
                throw new InvalidVINLengthException();
            if (Regex.IsMatch(dto.Vin, "[IOQioq]"))
                throw new ForbiddenCharacterException();
            if (!Regex.IsMatch(dto.Vin, @"^[A-HJ-NPR-Z0-9]{17}$"))
                throw new ForbiddenCharacterException("Số VIN chứa ký tự không hợp lệ hoặc sai định dạng ISO.");

            // 3.4 Quản lý Ngày tháng (Past Date Exception)
            if (!dto.RegistrationExpirationDate.HasValue || dto.RegistrationExpirationDate <= today)
                throw new PastDateException("registrationExpirationDate", "Hạn đăng kiểm không hợp lệ hoặc đã hết hạn.");
            
            if (!dto.InsuranceExpirationDate.HasValue || dto.InsuranceExpirationDate <= today)
                throw new PastDateException("insuranceExpirationDate", "Hạn bảo hiểm không hợp lệ hoặc đã hết hạn.");

            if (dto.BadgeExpirationDate.HasValue && dto.BadgeExpirationDate <= today)
                throw new PastDateException("badgeExpirationDate", "Hạn phù hiệu đã hết hạn.");

            // 3.5 Các thông tin khác
            if (string.IsNullOrWhiteSpace(dto.ChassisNumber)) throw new ArgumentException("Số khung (Chassis) không được để trống.");
            if (string.IsNullOrWhiteSpace(dto.EngineNumber)) throw new ArgumentException("Số máy (Engine) không được để trống.");
            if (string.IsNullOrWhiteSpace(dto.ReceiptImageUrl)) throw new ArgumentException("Vui lòng tải lên ảnh chứng minh khi nhận xe.");
            
            int currentlyReceivedByBranch = await _context.VehicleReceptionRecords
                .CountAsync(r => r.PurchaseProposalId == dto.PurchaseProposalId && r.BranchId == dto.BranchId && r.Status != VehicleReceptionRecord.RejectedStatus);

            if (currentlyReceivedByBranch >= branchProposedQuantity)
                throw new ArgumentException($"Chi nhánh này chỉ được tiếp nhận tối đa {branchProposedQuantity} xe theo đề xuất. Đã nhận đủ số lượng.");

            bool isPlateInVehicle = await _context.Vehicles.AnyAsync(v => v.LicensePlate == cleanPlate);
            if (isPlateInVehicle)
                throw new DuplicateVehicleException("licensePlate", cleanPlate);

            bool isVinInVehicle = await _context.Vehicles.AnyAsync(v => v.Vin == dto.Vin);
            if(isVinInVehicle)
                throw new DuplicateVehicleException("vin", dto.Vin);

            bool isPlateInReception = await _context.VehicleReceptionRecords
                .AnyAsync(r => r.LicensePlate == cleanPlate && r.Status != "Rejected" && r.PurchaseProposalId != dto.PurchaseProposalId);
            if (isPlateInReception)
                throw new DuplicateVehicleException("licensePlate", cleanPlate);

            // 4. TẠO RECORD TIẾP NHẬN
            var record = new VehicleReceptionRecord();
            record.InitCreate(dto.PurchaseProposalId, dto.BranchId, operatorId, requestedDate);
            
            // XÁC NHẬN TRẠNG THÁI CHỜ THANH TOÁN (Kế toán xử lý tiếp)
            record.Status = VehicleReceptionRecord.ReceivedPendingPaymentStatus;

            record.UpdateReceptionDetails(
                cleanPlate, dto.Vin, dto.ChassisNumber, dto.EngineNumber,
                dto.RegistrationExpirationDate, dto.InsuranceExpirationDate, dto.BadgeType,
                dto.BadgeExpirationDate, dto.FuelNorm ?? vehicleDetail?.FuelNorm, dto.ReceiptImageUrl, dto.Notes,
                dto.YearManufacture, dto.Mileage);
            
            record.Version = dto.Version;

            var createdRecord = await _repository.AddAsync(record);

            int totalProposedQuantity = proposal.BulkPurchaseDetails.Sum(d => d.ProposedQuantity ?? 0);
            int totalReceivedInDb = await _context.VehicleReceptionRecords
                .CountAsync(r => r.PurchaseProposalId == dto.PurchaseProposalId && r.Status != VehicleReceptionRecord.RejectedStatus);

            // Nếu đã nhận / đang nhận chiếc cuối cùng (bao gồm bản ghi đang tạo này) thì mới chuyển status của Proposal
            if (totalReceivedInDb + 1 >= totalProposedQuantity)
            {
                proposal.MarkAsReceived(dto.LicensePlate, operatorId); 
            }

            // 5. ĐỒNG BỘ DỮ LIỆU - TỰ ĐỘNG TẠO XE TRONG KHO TÀI SẢN
            // 5.1 Tìm hoặc tạo thông tin Model xe (để hiển thị Hãng/Dòng xe)
            var model = await _context.VehicleModels
                .FirstOrDefaultAsync(m => m.Manufacturer == vehicleDetail.Manufacturer && m.ModelName == vehicleDetail.Version);

            if (model == null)
            {
                model = new VehicleModel
                {
                    Manufacturer = vehicleDetail.Manufacturer,
                    ModelName = vehicleDetail.Version,
                    Seats = vehicleDetail.Seats,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _context.VehicleModels.Add(model);
                await _context.SaveChangesAsync();
            }

            var newVehicle = new Vehicle
            {
                LicensePlate = dto.LicensePlate,
                Status = "Active", // Gán trạng thái hoạt động
                CurrentBranchId = dto.BranchId,
                PurchaseDate = DateOnly.FromDateTime(DateTime.Now),
                OriginalCost = vehicleDetail?.UnitPrice ?? 0,
                CurrentValue = vehicleDetail?.UnitPrice ?? 0,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,

                // Gán Model và Năm sản xuất
                ModelId = model.Id,
                YearManufacture = dto.YearManufacture ?? DateTime.Now.Year,

                // Map toàn bộ thông tin FMS
                Vin = dto.Vin,
                ChassisNumber = dto.ChassisNumber,
                EngineNumber = dto.EngineNumber,
                RegistrationExpirationDate = dto.RegistrationExpirationDate,
                InsuranceExpirationDate = dto.InsuranceExpirationDate,
                BadgeType = dto.BadgeType,
                BadgeExpirationDate = dto.BadgeExpirationDate,
                FuelNorm = dto.FuelNorm ?? vehicleDetail?.FuelNorm,
                Mileage = dto.Mileage ?? 0
            };
            _context.Vehicles.Add(newVehicle);

            await _context.SaveChangesAsync(); // Lưu cả record và vehicle mới

            return MapToDto(createdRecord);
        }

        public async Task<VehicleReceptionRecordDto> UpdateAsync(int id, CreateVehicleReceptionDto dto)
        {
            var record = await _repository.GetByIdAsync(id)
                ?? throw new ArgumentException("Reception record not found");
        
            var today = DateOnly.FromDateTime(DateTime.Now);

            // 1. Biển số xe (Regex + Chuẩn hóa)
            if (string.IsNullOrWhiteSpace(dto.LicensePlate)) 
                throw new InvalidLicensePlateException("Biển số xe không được để trống.");
            
            var cleanPlate = dto.LicensePlate.ToUpper().Replace(" ", "");
            if (!Regex.IsMatch(cleanPlate, @"^[0-9]{2}[A-Z]{1,2}-[0-9]{3,5}(\.[0-9]{2})?$"))
                throw new InvalidLicensePlateException();

            // 2. Số VIN (ISO Standard)
            if (string.IsNullOrWhiteSpace(dto.Vin)) 
                throw new InvalidVINLengthException("Số VIN không được để trống.");
            if (dto.Vin.Length != 17)
                throw new InvalidVINLengthException();
            if (Regex.IsMatch(dto.Vin, "[IOQioq]"))
                throw new ForbiddenCharacterException();
            if (!Regex.IsMatch(dto.Vin, @"^[A-HJ-NPR-Z0-9]{17}$"))
                throw new ForbiddenCharacterException("Số VIN chứa ký tự không hợp lệ hoặc sai định dạng ISO.");

            // 3. Quản lý Ngày tháng (Past Date Exception)
            if (dto.RegistrationExpirationDate.HasValue && dto.RegistrationExpirationDate <= today)
                throw new PastDateException("registrationExpirationDate");
            if (dto.InsuranceExpirationDate.HasValue && dto.InsuranceExpirationDate <= today)
                throw new PastDateException("insuranceExpirationDate");

            // 4. KIỂM TRA TRÙNG LẶP
            if (cleanPlate != record.LicensePlate)
            {
                if (await _context.Vehicles.AnyAsync(v => v.LicensePlate == cleanPlate))
                    throw new DuplicateVehicleException("licensePlate", cleanPlate);
                if (await _context.VehicleReceptionRecords.AnyAsync(r => r.LicensePlate == cleanPlate && r.Id != id && r.Status != "Rejected"))
                    throw new DuplicateVehicleException("licensePlate", cleanPlate);
            }
            if (dto.Vin != record.Vin)
            {
                if (await _context.Vehicles.AnyAsync(v => v.Vin == dto.Vin))
                    throw new DuplicateVehicleException("vin", dto.Vin);
            }

            record.UpdateReceptionDetails(
                cleanPlate, dto.Vin, dto.ChassisNumber, dto.EngineNumber,
                dto.RegistrationExpirationDate, dto.InsuranceExpirationDate, dto.BadgeType,
                dto.BadgeExpirationDate, dto.FuelNorm ?? record.FuelNorm, dto.ReceiptImageUrl, dto.Notes,
                dto.YearManufacture, dto.Mileage);

            var updated = await _repository.UpdateAsync(record);

            // 6. CẬP NHẬT XE TRONG KHO TÀI SẢN (NẾU ĐÃ TỒN TẠI)
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.LicensePlate == record.LicensePlate);
            if (vehicle != null)
            {
                // Lấy Manufacturer từ đề xuất gốc (vì DTO không gửi lên)
                var vehicleDetail = await _context.BulkPurchaseDetails
                    .FirstOrDefaultAsync(d => d.PurchaseProposalId == record.PurchaseProposalId && 
                                            d.BranchId == record.BranchId && 
                                            d.Version == record.Version);
                
                string manufacturer = vehicleDetail?.Manufacturer ?? "";

                // Cập nhật lại ModelId nếu Hãng/Dòng xe thay đổi
                var model = await _context.VehicleModels
                    .FirstOrDefaultAsync(m => m.Manufacturer == manufacturer && m.ModelName == record.Version);

                if (model == null && !string.IsNullOrEmpty(manufacturer))
                {
                    model = new VehicleModel
                    {
                        Manufacturer = manufacturer,
                        ModelName = record.Version,
                        Seats = vehicleDetail?.Seats,
                        CreatedAt = DateTime.Now,
                        UpdatedAt = DateTime.Now
                    };
                    _context.VehicleModels.Add(model);
                    await _context.SaveChangesAsync();
                }

                if (model != null) vehicle.ModelId = model.Id;

                vehicle.Vin = dto.Vin;
                vehicle.ChassisNumber = dto.ChassisNumber;
                vehicle.EngineNumber = dto.EngineNumber;
                vehicle.RegistrationExpirationDate = dto.RegistrationExpirationDate;
                vehicle.InsuranceExpirationDate = dto.InsuranceExpirationDate;
                vehicle.BadgeType = dto.BadgeType;
                vehicle.BadgeExpirationDate = dto.BadgeExpirationDate;
                vehicle.FuelNorm = dto.FuelNorm;
                vehicle.YearManufacture = dto.YearManufacture;
                vehicle.Mileage = dto.Mileage;
                vehicle.UpdatedAt = DateTime.Now;
                await _context.SaveChangesAsync();
            }

            return MapToDto(updated);
        }

        public async Task<VehicleReceptionRecordDto> CompleteAsync(int id)
        {
            var record = await _repository.GetByIdAsync(id)
                ?? throw new Exception("Reception record not found");

            record.Complete();
            var updated = await _repository.UpdateAsync(record);
            return MapToDto(updated);
        }

        public async Task<VehicleReceptionRecordDto> RejectAsync(int id, string? reason)
        {
            var record = await _repository.GetByIdAsync(id)
                ?? throw new Exception("Reception record not found");

            record.Reject(reason);
            var updated = await _repository.UpdateAsync(record);
            return MapToDto(updated);
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        private VehicleReceptionRecordDto MapToDto(VehicleReceptionRecord record)
        {
            return new VehicleReceptionRecordDto
            {
                Id = record.Id,
                ProposalId = record.PurchaseProposalId,
                BranchId = record.BranchId,
                BranchName = record.Branch?.Name,
                OperatorId = record.OperatorId,
                OperatorName = record.Operator?.Name,
                RequestedDate = record.RequestedDate,
                ReceivedDate = record.ReceivedDate,
                LicensePlate = record.LicensePlate,
                ChassisNumber = record.ChassisNumber,
                EngineNumber = record.EngineNumber,
                ReceiptImageUrl = record.ReceiptImageUrl,
                Notes = record.Notes,
                Status = record.Status,
                DaysDelay = record.GetDaysDelay(),
                IsLate = record.IsLate(),
                YearManufacture = record.YearManufacture,
                Mileage = record.Mileage,
                CreatedAt = record.CreatedAt,
                UpdatedAt = record.UpdatedAt
            };
        }
    }
}
