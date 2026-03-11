using Data.Repositories.Interfaces;
using Models.DTO.PurchaseProposal;
using Models.Models;
using Service.Services.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Service.Services.Implementations
{
    public class VehicleReceptionService : IVehicleReceptionService
    {
        private readonly IVehicleReceptionRepository _repository;

        public VehicleReceptionService(IVehicleReceptionRepository repository)
        {
            _repository = repository;
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
            return MapToDto(created);
        }

        public async Task<VehicleReceptionRecordDto> UpdateAsync(int id, CreateVehicleReceptionDto dto)
        {
            var record = await _repository.GetByIdAsync(id)
                ?? throw new Exception("Reception record not found");

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
