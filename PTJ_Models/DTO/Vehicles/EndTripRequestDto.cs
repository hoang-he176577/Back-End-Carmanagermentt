using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Models.DTO.Vehicles
{
    public class EndTripRequestDto
    {
        [Required]
        public decimal EndMileage { get; set; }

        public bool? IsStopDifferent { get; set; }
        public string? ActualStop { get; set; }
        public string? StopDeviationReason { get; set; }

        public string? OvertimeReason { get; set; }
        public int? ExtensionDays { get; set; }
        public int? ExtensionHours { get; set; }
        public int? ExtensionMinutes { get; set; }
    }
}
