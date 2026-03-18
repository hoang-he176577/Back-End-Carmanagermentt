using System;

namespace Models.DTO.Vehicles
{
    public class ManageVehicleTripDto
    {
        public int VehicleId { get; set; }
        public string? LicensePlate { get; set; }
        public string? Status { get; set; }

        public int? CurrentBranchId { get; set; }
        public string? CurrentBranchName { get; set; }

        public int? CurrentDriverId { get; set; }
        public string? CurrentDriverName { get; set; }

        public bool IsMoving { get; set; }
        public int? CurrentTripId { get; set; }
        public DateTime? CurrentTripStartTime { get; set; }
        public decimal? CurrentTripStartMileage { get; set; }
        public string? CurrentTripOrigin { get; set; }
        public string? CurrentTripDestination { get; set; }
        public bool IsOverDuration { get; set; }
    }
}
