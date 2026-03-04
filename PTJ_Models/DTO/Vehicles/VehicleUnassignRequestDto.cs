using System;
using System.ComponentModel.DataAnnotations;

namespace Models.DTO.Vehicles;

public sealed class VehicleUnassignRequestDto
{
    [Required]
    public int VehicleId { get; set; }

    public DateOnly? UnassignDate { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}

