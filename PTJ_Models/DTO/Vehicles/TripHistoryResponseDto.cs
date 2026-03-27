using System;

namespace Models.DTO.Vehicles
{
    public class TripHistoryResponseDto
    {
        public int TripId { get; set; }
        public int TransferPlanId { get; set; }
        public string? FromBranchName { get; set; }
        public string? ToBranchName { get; set; }
        public int? VehicleId { get; set; }
        public string? VehicleLicensePlate { get; set; }
        public int? DriverId { get; set; }
        public string? DriverName { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal? StartMileage { get; set; }
        public decimal? EndMileage { get; set; }
        public string? Origin { get; set; }
        public string? Destination { get; set; }
        public int? PlannedDurationMinutes { get; set; }
        public bool? IsStopDifferent { get; set; }
        public string? ActualStop { get; set; }
        public string? StopDeviationReason { get; set; }
        public string? OvertimeReason { get; set; }
        public int? ExtensionMinutes { get; set; }
    }
}
