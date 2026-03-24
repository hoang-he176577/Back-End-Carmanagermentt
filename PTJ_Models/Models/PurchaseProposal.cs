using System;
using System.Collections.Generic;

namespace Models.Models;

public partial class PurchaseProposal
{
    public int Id { get; set; }

    public int? ProposerId { get; set; }

    public int? ManagerId { get; set; }

    public int? ChiefAccountantId { get; set; }

    public DateOnly? CreatedDate { get; set; }

    public DateOnly? ApprovedDate { get; set; }

    public string? Status { get; set; }

    public decimal? ProposedCost { get; set; }

    public string? Description { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<BulkPurchaseDetail> BulkPurchaseDetails { get; set; } = new List<BulkPurchaseDetail>();

    public virtual User? ChiefAccountant { get; set; }

    public virtual User? Manager { get; set; }

    public virtual User? Proposer { get; set; }

    public virtual ICollection<VehicleReceptionRecord> VehicleReceptionRecords { get; set; } = new List<VehicleReceptionRecord>();
}
