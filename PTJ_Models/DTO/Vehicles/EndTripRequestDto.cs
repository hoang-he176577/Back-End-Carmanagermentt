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
        [Required]
        public string Destination { get; set; }
    }
}
