using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTO.Vehicles
{
    public class TripHistoryResponseDto
    {
        public int TripId { get; set; }

        public int VehicleId { get; set; }

        public int DriverId { get; set; }
        public string DriverName { get; set; } = string.Empty;

        public DateTime StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public decimal StartMileage { get; set; }

        public decimal? EndMileage { get; set; }

        public string? Origin { get; set; }

        public string? Destination { get; set; }

        public string? Purpose { get; set; }
    }
}
