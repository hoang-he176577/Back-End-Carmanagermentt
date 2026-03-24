using System;
using System.Collections.Generic;

namespace Models.Models;

public partial class DriverTransferRequest
{
    public int Id { get; set; }

    public int RequestingBranchId { get; set; }

    public int RequestedQuantity { get; set; }

    public int FulfilledQuantity { get; set; }

    public string? Status { get; set; }

    public string? Reason { get; set; }

    public int CreatedByUserId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual User? CreatedByUser { get; set; }

    public virtual Branch? RequestingBranch { get; set; }

    public virtual ICollection<DriverTransferDetail> TransferDetails { get; set; } = new List<DriverTransferDetail>();
}
