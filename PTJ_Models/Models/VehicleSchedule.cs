using System;
using System.Collections.Generic;

namespace Models.Models;

public partial class VehicleSchedule
{
    public int Id { get; set; }

    public int VehicleId { get; set; }

    public int DriverId { get; set; }

    public int BranchId { get; set; }

    public DateTime PlannedStartTime { get; set; }

    public DateTime PlannedEndTime { get; set; }

    public DateTime? ActualStartTime { get; set; }

    public DateTime? ActualEndTime { get; set; }

    public string? Origin { get; set; }

    public string? Destination { get; set; }

    public string Status { get; set; } = null!;

    public int? ExtensionMinutes { get; set; }

    public string? ExtensionReason { get; set; }

    public int? SwapFromScheduleId { get; set; }

    public int? SwappedVehicleId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual Driver Driver { get; set; } = null!;

    public virtual Vehicle Vehicle { get; set; } = null!;

    public virtual Vehicle? SwappedVehicle { get; set; }

    public virtual VehicleSchedule? SwapFromSchedule { get; set; }

    public virtual ICollection<VehicleSchedule> SwapToSchedules { get; set; } = new List<VehicleSchedule>();
}
