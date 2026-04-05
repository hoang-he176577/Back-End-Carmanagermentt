using System.Collections.Generic;

namespace Models.Models;

public partial class Vehicle
{
    public virtual ICollection<VehicleSchedule> VehicleSchedules { get; set; } = new List<VehicleSchedule>();

    public virtual ICollection<VehicleSchedule> VehicleSchedulesAsSwap { get; set; } = new List<VehicleSchedule>();
}
