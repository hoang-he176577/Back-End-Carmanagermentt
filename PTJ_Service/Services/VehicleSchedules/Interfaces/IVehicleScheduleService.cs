using Models.DTO.Schedules;

namespace Service.Services.VehicleSchedules.Interfaces
{
    public interface IVehicleScheduleService
    {
        Task<List<VehicleScheduleDto>> GetSchedulesAsync(
            int branchId,
            DateTime? from,
            DateTime? to,
            int? vehicleId,
            int? driverId,
            int? modelId,
            int? seats,
            string? status);

        Task<VehicleScheduleDto> CreateScheduleAsync(int branchId, int actorUserId, VehicleScheduleCreateRequestDto request);

        Task<VehicleScheduleDto> UpdateScheduleAsync(int scheduleId, int branchId, int actorUserId, VehicleScheduleUpdateRequestDto request);

        Task<VehicleScheduleExtendResultDto> ExtendScheduleAsync(int scheduleId, int branchId, int actorUserId, VehicleScheduleExtendRequestDto request);

        Task<List<VehicleScheduleAvailabilityDto>> GetAvailableVehiclesAsync(int branchId, int? modelId, DateTime start, DateTime end);

        Task<List<DriverAvailabilityDto>> GetAvailableDriversAsync(int branchId, DateTime start, DateTime end);

        Task<VehicleScheduleDto> StartScheduleAsync(int scheduleId, int branchId, int actorUserId, VehicleScheduleActionRequestDto request);

        Task<VehicleScheduleDto> EndScheduleAsync(int scheduleId, int branchId, int actorUserId, VehicleScheduleActionRequestDto request);

        Task<List<VehicleScheduleAuditDto>> GetAuditsByScheduleAsync(int scheduleId, int branchId);

        Task<List<VehicleScheduleAuditDto>> GetRecentAuditsAsync(int branchId, DateTime? from, DateTime? to);
    }
}
