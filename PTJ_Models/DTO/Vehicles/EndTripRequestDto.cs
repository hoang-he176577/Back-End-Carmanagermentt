using System.ComponentModel.DataAnnotations;

namespace Models.DTO.Vehicles
{
    public class EndTripRequestDto
    {
        public int EndedBy { get; set; }

        public decimal EndMileage { get; set; }

        /// <summary>Required if arrival is late vs planned date.</summary>
        public string? Note { get; set; }
    }
}
