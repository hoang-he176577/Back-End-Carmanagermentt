using System;
using System.Collections.Generic;

namespace Models.Models;

public partial class MaintenanceRequest
{
    public int Id { get; set; }

    public int? VehicleId { get; set; }

    public int? OperatorId { get; set; }

    public DateOnly? RequestDate { get; set; }

    public string? Description { get; set; }

    public decimal? EstimatedCost { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public string MaintenanceType { get; set; } = null!;

    public int? AccountantId { get; set; }

    public DateOnly? ApprovedDate { get; set; }

    public decimal? ActualCost { get; set; }

    public DateOnly? CompletionDate { get; set; }

    public string? ApprovalNote { get; set; }

    public string? RejectionReason { get; set; }

    public virtual User? Accountant { get; set; }

    public virtual User? Operator { get; set; }

    public virtual ICollection<OverBudgetRepairProposal> OverBudgetRepairProposals { get; set; } = new List<OverBudgetRepairProposal>();

    public virtual Vehicle? Vehicle { get; set; }
}
