using System;
using System.Collections.Generic;

namespace Models.Models;

public partial class VehicleDriverHistory
{
    public int Id { get; set; }

    public int? VehicleId { get; set; }

    public int? DriverId { get; set; }

    public DateOnly? AssignDate { get; set; }

    public DateOnly? UnassignDate { get; set; }

    public string? Notes { get; set; }

    public virtual Driver? Driver { get; set; }

    public virtual Vehicle? Vehicle { get; set; }
}
