using Models.DTO.PurchaseProposal;
using Models.Models;

namespace Service.Services.Interfaces
{
    /// <summary>
    /// Service cho bảng đối chiếu xe
    /// </summary>
    public interface IVehicleReceptionService
    {
        /// <summary>
        /// Lấy thông tin bản ghi đối chiếu theo ID
        /// </summary>
        Task<VehicleReceptionRecordDto?> GetByIdAsync(int id);

        /// <summary>
        /// Lấy danh sách bản ghi đối chiếu theo ProposalId
        /// </summary>
        Task<List<VehicleReceptionRecordDto>> GetByProposalIdAsync(int proposalId);

        /// <summary>
        /// Lấy danh sách bản ghi đối chiếu cho chi nhánh
        /// </summary>
        Task<List<VehicleReceptionRecordDto>> GetByBranchIdAsync(int branchId);

        /// <summary>
        /// Lấy danh sách bản ghi chưa hoàn thành của chi nhánh
        /// </summary>
        Task<List<VehicleReceptionRecordDto>> GetPendingByBranchAsync(int branchId);

        /// <summary>
        /// Lấy tất cả bản ghi (dành cho Manager)
        /// </summary>
        Task<List<VehicleReceptionRecordDto>> GetAllAsync();

        /// <summary>
        /// Tạo bản ghi đối chiếu mới
        /// </summary>
        Task<VehicleReceptionRecordDto> CreateAsync(CreateVehicleReceptionDto dto, int operatorId, DateOnly requestedDate);

        /// <summary>
        /// Cập nhật thông tin đối chiếu xe
        /// </summary>
        Task<VehicleReceptionRecordDto> UpdateAsync(int id, CreateVehicleReceptionDto dto);

        /// <summary>
        /// Hoàn thành bản ghi đối chiếu
        /// </summary>
        Task<VehicleReceptionRecordDto> CompleteAsync(int id);

        /// <summary>
        /// Từ chối bản ghi đối chiếu
        /// </summary>
        Task<VehicleReceptionRecordDto> RejectAsync(int id, string? reason);

        /// <summary>
        /// Xóa bản ghi (soft delete)
        /// </summary>
        Task<bool> DeleteAsync(int id);
    }
}
