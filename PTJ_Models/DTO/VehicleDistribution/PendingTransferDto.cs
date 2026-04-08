namespace Models.DTO.VehicleDistribution
{
    public class PendingTransferDto
    {
        public int TransferPlanId { get; set; }
        public int? VehicleId { get; set; }
        public string? LicensePlate { get; set; }
        public int? FromBranchId { get; set; }
        public string? FromBranchName { get; set; }
        public int? ToBranchId { get; set; }
        public string? ToBranchName { get; set; }
        public int? DriverId { get; set; }
        public string? DriverName { get; set; }
        public decimal? CurrentMileage { get; set; }
        public DateTime? PlannedDepartureDate { get; set; }
        public DateTime? PlannedArrivalDate { get; set; }

        /// <summary>True = user is at source branch (can start trip). False = user is at destination branch (view only / can end trip).</summary>
        public bool IsSourceBranch { get; set; }

        /// <summary>The active trip ID (only set for InTransit transfers).</summary>
        public int? TripId { get; set; }

        /// <summary>Trip start time (only set for InTransit transfers).</summary>
        public DateTime? StartTime { get; set; }
    }
}
