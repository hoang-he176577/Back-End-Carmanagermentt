using System;

namespace Models.Models;

public partial class VehicleScheduleAudit
{
    public int Id { get; set; }

    public int ScheduleId { get; set; }

    public int ActorUserId { get; set; }

    public string Action { get; set; } = null!;

    public string? Note { get; set; }

    public string? DataJson { get; set; }

    public DateTime? CreatedAt { get; set; }

    public virtual User ActorUser { get; set; } = null!;

    public virtual VehicleSchedule Schedule { get; set; } = null!;
}
