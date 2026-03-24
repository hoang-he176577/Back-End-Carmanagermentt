using System;
using System.Collections.Generic;

namespace Models.Models;

public partial class TransferPlan
{
    public int Id { get; set; }

    public int? VehicleId { get; set; }

    public int? FromBranchId { get; set; }

    public int? ToBranchId { get; set; }

    public int? ManagerId { get; set; }

    public DateOnly? PlanDate { get; set; }

    public DateOnly? ExecutedDate { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public DateTime? CheckoutDate { get; set; }

    public int? CheckoutByUserId { get; set; }

    public DateTime? CheckinDate { get; set; }

    public int? CheckinByUserId { get; set; }
    public virtual Branch? FromBranch { get; set; }

    public virtual User? Manager { get; set; }

    public virtual Branch? ToBranch { get; set; }

    public virtual Vehicle? Vehicle { get; set; }

    public virtual User? CheckoutByUser { get; set; }

    public virtual User? CheckinByUser { get; set; }
}
