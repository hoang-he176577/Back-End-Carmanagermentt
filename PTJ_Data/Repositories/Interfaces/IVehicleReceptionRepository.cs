using Models.Models;

namespace Data.Repositories.Interfaces
{
    /// <summary>
    /// Repository cho bảng đối chiếu xe
    /// </summary>
    public interface IVehicleReceptionRepository
    {
        Task<VehicleReceptionRecord?> GetByIdAsync(int id);

        Task<List<VehicleReceptionRecord>> GetByProposalIdAsync(int proposalId);

        Task<List<VehicleReceptionRecord>> GetByBranchIdAsync(int branchId);

        Task<List<VehicleReceptionRecord>> GetPendingByBranchAsync(int branchId);

        /// <summary>
        /// Lấy tất cả bản ghi đối chiếu (dành cho Manager xem tất cả chi nhánh)
        /// </summary>
        Task<List<VehicleReceptionRecord>> GetAllAsync();

        /// <summary>
        /// Lấy bản ghi đối chiếu theo trạng thái
        /// </summary>
        Task<List<VehicleReceptionRecord>> GetByStatusAsync(string status);

        Task<VehicleReceptionRecord> AddAsync(VehicleReceptionRecord record);

        Task<VehicleReceptionRecord> UpdateAsync(VehicleReceptionRecord record);

        Task<bool> DeleteAsync(int id);

        Task<bool> ExistsAsync(int id);
    }
}
