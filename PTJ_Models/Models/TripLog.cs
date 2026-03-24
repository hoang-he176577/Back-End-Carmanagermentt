
namespace Models.Models
{
    public partial class TripLog
    {
        public int Id { get; set; }
        public int? VehicleId { get; set; }
        public int? DriverId { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public decimal? StartMileage { get; set; }
        public decimal? EndMileage { get; set; }
        public string? Origin { get; set; }
        public string? Destination { get; set; }
        public string? Purpose { get; set; }
        public int? StartedBy { get; set; }
        public int? EndedBy { get; set; }
        public DateTime? CreatedAt { get; set; }

        public virtual Driver? Driver { get; set; }
        public virtual Vehicle? Vehicle { get; set; }
    }
}
