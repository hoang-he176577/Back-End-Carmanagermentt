using Data.Repositories.Interfaces;
using Models.DTO.PurchaseProposal;
using Models.Models;
using Service.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
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
            var proposal = await _context.PurchaseProposals.FindAsync(dto.PurchaseProposalId);
            if (proposal == null) 
                throw new Exception($"Đề xuất mua #{dto.PurchaseProposalId} không tồn tại trong hệ thống.");
            if (proposal.Status != "Approved") 
                throw new Exception($"Đề xuất #{dto.PurchaseProposalId} chưa được duyệt hoặc đã xử lý xong.");

            var branchExists = await _context.Branches.AnyAsync(b => b.Id == dto.BranchId);
            if (!branchExists) 
                throw new Exception($"Chi nhánh #{dto.BranchId} không tồn tại.");

            // 2. KIỂM TRA DỮ LIỆU ĐẦU VÀO KHÔNG ĐƯỢC TRỐNG
            if (string.IsNullOrWhiteSpace(dto.LicensePlate)) throw new Exception("Biển số xe không được để trống.");
            if (string.IsNullOrWhiteSpace(dto.ChassisNumber)) throw new Exception("Số khung (Chassis) không được để trống.");
            if (string.IsNullOrWhiteSpace(dto.EngineNumber)) throw new Exception("Số máy (Engine) không được để trống.");
            if (string.IsNullOrWhiteSpace(dto.ReceiptImageUrl)) throw new Exception("Vui lòng tải lên ảnh chứng minh khi nhận xe.");

            // 3. KIỂM TRA TRÙNG BIỂN SỐ (UNIQUE CONSTRAINT)
            bool isPlateInVehicle = await _context.Vehicles.AnyAsync(v => v.LicensePlate == dto.LicensePlate);
            if (isPlateInVehicle) 
                throw new Exception($"Biển số xe {dto.LicensePlate} đã tồn tại trong kho tài sản!");

            bool isPlateInReception = await _context.VehicleReceptionRecords
                .AnyAsync(r => r.LicensePlate == dto.LicensePlate && r.Status != "Rejected" && r.PurchaseProposalId != dto.PurchaseProposalId);
            if (isPlateInReception) 
                throw new Exception($"Biển số xe {dto.LicensePlate} đang được chờ xử lý ở một đề xuất khác!");

            var record = new VehicleReceptionRecord();
            record.InitCreate(dto.PurchaseProposalId, dto.BranchId, operatorId, requestedDate);

            // Nếu có thông tin xe, cập nhật ngay
            if (!string.IsNullOrEmpty(dto.LicensePlate))
            {
                record.UpdateReceptionDetails(
                    dto.LicensePlate,
                    dto.ChassisNumber,
                    dto.EngineNumber,
                    dto.ReceiptImageUrl,
                    dto.Notes);
            }

            var created = await _repository.AddAsync(record);

            // TỰ ĐỘNG CHUYỂN TRẠNG THÁI CỦA ĐỀ XUẤT SANG CHỜ THANH TOÁN
            proposal.MarkAsReceived(dto.LicensePlate, operatorId);
            await _context.SaveChangesAsync();

            return MapToDto(created);
        }

        public async Task<VehicleReceptionRecordDto> UpdateAsync(int id, CreateVehicleReceptionDto dto)
        {
            var record = await _repository.GetByIdAsync(id)
                ?? throw new Exception("Reception record not found");

            if (string.IsNullOrWhiteSpace(dto.LicensePlate)) throw new Exception("Biển số xe không được để trống.");
            if (string.IsNullOrWhiteSpace(dto.ChassisNumber)) throw new Exception("Số khung (Chassis) không được để trống.");
            if (string.IsNullOrWhiteSpace(dto.EngineNumber)) throw new Exception("Số máy (Engine) không được để trống.");

            // KIỂM TRA TRÙNG BIỂN SỐ KHI CẬP NHẬT (NẾU ĐỔI BIỂN KHÁC)
            if (dto.LicensePlate != record.LicensePlate)
            {
                bool isPlateInVehicle = await _context.Vehicles.AnyAsync(v => v.LicensePlate == dto.LicensePlate);
                if (isPlateInVehicle) 
                    throw new Exception($"Biển số xe {dto.LicensePlate} đã tồn tại trong kho tài sản!");

                bool isPlateInReception = await _context.VehicleReceptionRecords
                    .AnyAsync(r => r.LicensePlate == dto.LicensePlate && r.Id != id && r.Status != "Rejected");
                if (isPlateInReception) 
                    throw new Exception($"Biển số xe {dto.LicensePlate} đang được chờ xử lý ở một đề xuất khác!");
            }

            record.UpdateReceptionDetails(
                dto.LicensePlate,
                dto.ChassisNumber,
                dto.EngineNumber,
                dto.ReceiptImageUrl,
                dto.Notes);

            var updated = await _repository.UpdateAsync(record);
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
                CreatedAt = record.CreatedAt,
                UpdatedAt = record.UpdatedAt
            };
        }
    }
}
