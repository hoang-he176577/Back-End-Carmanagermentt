using System;
using System.Collections.Generic;

namespace Models.Models;

public partial class OverBudgetRepairProposal
{
    public int Id { get; set; }

    public int? MaintenanceId { get; set; }

    public int? ManagerId { get; set; }

    public string? Status { get; set; }

    public decimal? OverBudgetAmount { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual MaintenanceRequest? Maintenance { get; set; }

    public virtual User? Manager { get; set; }
}
