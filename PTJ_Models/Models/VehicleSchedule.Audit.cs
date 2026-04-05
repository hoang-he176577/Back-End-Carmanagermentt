using System.Collections.Generic;

namespace Models.Models;

public partial class VehicleSchedule
{
    public virtual ICollection<VehicleScheduleAudit> VehicleScheduleAudits { get; set; } = new List<VehicleScheduleAudit>();
}
