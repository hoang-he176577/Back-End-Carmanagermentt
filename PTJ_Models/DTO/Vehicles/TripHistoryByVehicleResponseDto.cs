using System.Collections.Generic;

namespace Models.DTO.Vehicles
{
    public class TripHistoryByVehicleResponseDto
    {
        public int VehicleId { get; set; }
        public string? LicensePlate { get; set; }
        public string? Status { get; set; }

        public int? CurrentBranchId { get; set; }
        public string? CurrentBranchName { get; set; }

        public int? CurrentDriverId { get; set; }
        public string? CurrentDriverName { get; set; }

        public List<TripHistoryResponseDto> Trips { get; set; } = new();
    }
}
