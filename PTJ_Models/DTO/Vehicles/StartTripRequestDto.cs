using System.ComponentModel.DataAnnotations;

namespace Models.DTO.Vehicles
{
    public class StartTripRequestDto
    {
        public int VehicleId { get; set; }

        public int DriverId { get; set; }

        public DateTime? StartTime { get; set; }

        public decimal StartMileage { get; set; }

        public string? Origin { get; set; }

        public string? Destination { get; set; }

        public string? Purpose { get; set; }
        public int OperatorId { get; set; }
    }
}
