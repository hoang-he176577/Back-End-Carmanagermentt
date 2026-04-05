using System.Collections.Generic;

namespace Models.Models;

public partial class Driver
{
    public virtual ICollection<VehicleSchedule> VehicleSchedules { get; set; } = new List<VehicleSchedule>();
}
