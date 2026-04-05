using System.Collections.Generic;

namespace Models.Models;

public partial class Branch
{
    public virtual ICollection<VehicleSchedule> VehicleSchedules { get; set; } = new List<VehicleSchedule>();
}
