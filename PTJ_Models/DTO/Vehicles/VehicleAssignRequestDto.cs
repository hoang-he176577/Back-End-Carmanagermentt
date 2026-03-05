using System;
using System.ComponentModel.DataAnnotations;

namespace Models.DTO.Vehicles;

public sealed class VehicleAssignRequestDto
{
    [Required]
    public int DriverId { get; set; }

    public DateOnly? AssignDate { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}

