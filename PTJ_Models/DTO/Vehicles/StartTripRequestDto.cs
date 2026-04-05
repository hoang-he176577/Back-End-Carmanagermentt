using System.ComponentModel.DataAnnotations;

namespace Models.DTO.Vehicles
{
    public class StartTripRequestDto
    {
        [Required]
        public int VehicleId { get; set; }

        [Required]
        public int DriverId { get; set; }

        [Required]
        public decimal StartMileage { get; set; }

        [Required]
        public string Origin { get; set; } 

        [Required]
        public string Destination { get; set; } 

        public string? Purpose { get; set; }
        public int? PlannedDurationDays { get; set; }
        public int? PlannedDurationHours { get; set; }
        public int? PlannedDurationMinutes { get; set; }
        public int OperatorId { get; set; }
    }
}
