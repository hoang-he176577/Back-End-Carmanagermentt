using System;
using System.Collections.Generic;

namespace Models.DTO.Schedules
{
    public class VehicleScheduleDto
    {
        public int Id { get; set; }
        public int VehicleId { get; set; }
        public string? VehicleLicensePlate { get; set; }
        public int? VehicleModelId { get; set; }
        public string? VehicleModelName { get; set; }
        public string? VehicleManufacturer { get; set; }
        public int DriverId { get; set; }
        public string? DriverName { get; set; }
        public int BranchId { get; set; }
        public string? BranchName { get; set; }
        public DateTime PlannedStartTime { get; set; }
        public DateTime PlannedEndTime { get; set; }
        public DateTime? ActualStartTime { get; set; }
        public DateTime? ActualEndTime { get; set; }
        public string? Origin { get; set; }
        public string? Destination { get; set; }
        public string Status { get; set; } = "Planned";
        public int? ExtensionMinutes { get; set; }
        public string? ExtensionReason { get; set; }
        public int? SwapFromScheduleId { get; set; }
        public int? SwappedVehicleId { get; set; }
        public string? SwappedVehicleLicensePlate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class VehicleScheduleCreateRequestDto
    {
        public int VehicleId { get; set; }
        public int DriverId { get; set; }
        public DateTime PlannedStartTime { get; set; }
        public DateTime PlannedEndTime { get; set; }
        public string? Origin { get; set; }
        public string? Destination { get; set; }
        public int? BranchId { get; set; }
    }

    public class VehicleScheduleUpdateRequestDto
    {
        public int? VehicleId { get; set; }
        public int? DriverId { get; set; }
        public DateTime? PlannedStartTime { get; set; }
        public DateTime? PlannedEndTime { get; set; }
        public string? Origin { get; set; }
        public string? Destination { get; set; }
        public string? Status { get; set; }
    }

    public class VehicleScheduleExtendRequestDto
    {
        public int ExtensionMinutes { get; set; }
        public string? ExtensionReason { get; set; }
        public bool AllowSwap { get; set; }
        public int? SwapVehicleId { get; set; }
        public bool AllowDriverSwap { get; set; }
        public int? SwapDriverId { get; set; }
    }

    public class VehicleScheduleAvailabilityDto
    {
        public int VehicleId { get; set; }
        public string? LicensePlate { get; set; }
        public int? ModelId { get; set; }
        public string? ModelName { get; set; }
        public string? Manufacturer { get; set; }
        public string? BranchName { get; set; }
        public string? Status { get; set; }
        public int? CurrentDriverId { get; set; }
    }

    public class DriverAvailabilityDto
    {
        public int DriverId { get; set; }
        public string? DriverName { get; set; }
        public string? Phone { get; set; }
        public string? Email { get; set; }
        public string? BranchName { get; set; }
        public string? Status { get; set; }
    }

    public class VehicleScheduleConflictDto
    {
        public int ScheduleId { get; set; }
        public DateTime PlannedStartTime { get; set; }
        public DateTime PlannedEndTime { get; set; }
        public int VehicleId { get; set; }
        public string? VehicleLicensePlate { get; set; }
    }

    public class VehicleScheduleExtendResultDto
    {
        public bool Extended { get; set; }
        public VehicleScheduleDto? Schedule { get; set; }
        public VehicleScheduleConflictDto? Conflict { get; set; }
        public List<VehicleScheduleAvailabilityDto> AvailableVehicles { get; set; } = new();
        public List<DriverAvailabilityDto> AvailableDrivers { get; set; } = new();
    }

    public class VehicleScheduleAuditDto
    {
        public int Id { get; set; }
        public int ScheduleId { get; set; }
        public int ActorUserId { get; set; }
        public string? ActorName { get; set; }
        public string Action { get; set; } = string.Empty;
        public string? Note { get; set; }
        public string? DataJson { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
    public class VehicleScheduleActionRequestDto
    {
        public DateTime? ActualTime { get; set; }
        public string? Note { get; set; }
    }
}






