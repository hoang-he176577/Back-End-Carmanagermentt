using Models.Models;

namespace Data.Repositories.VehicleSchedules.Interfaces
{
    public interface IVehicleScheduleRepository
    {
        Task<VehicleSchedule?> GetByIdAsync(int id);
        Task<VehicleSchedule> CreateAsync(VehicleSchedule schedule);
        Task UpdateAsync(VehicleSchedule schedule);

        Task<List<VehicleSchedule>> GetSchedulesAsync(
            int branchId,
            DateTime? from,
            DateTime? to,
            int? vehicleId,
            int? driverId,
            int? modelId,
            int? seats,
            string? status);

        Task<List<Vehicle>> GetAvailableVehiclesAsync(int branchId, int? modelId, DateTime start, DateTime end);
        Task<List<Driver>> GetAvailableDriversAsync(int branchId, DateTime start, DateTime end);

        Task<Vehicle?> GetVehicleWithModelAsync(int vehicleId);
        Task<Driver?> GetDriverAsync(int driverId);
        Task<Branch?> GetBranchAsync(int branchId);
        Task AddAuditAsync(VehicleScheduleAudit audit);
        Task<List<VehicleScheduleAudit>> GetAuditsByScheduleAsync(int scheduleId);
        Task<List<VehicleScheduleAudit>> GetRecentAuditsAsync(int branchId, DateTime? from, DateTime? to);

        Task<VehicleSchedule?> GetLastScheduleBeforeAsync(int vehicleId, DateTime startTime, int? excludeScheduleId);

        Task<bool> HasVehicleOverlapAsync(int vehicleId, DateTime start, DateTime end, int? excludeScheduleId);
        Task<bool> HasDriverOverlapAsync(int driverId, DateTime start, DateTime end, int? excludeScheduleId);

        Task<VehicleSchedule?> GetNextScheduleAsync(int vehicleId, DateTime afterTime, int? excludeScheduleId);
    }
}
