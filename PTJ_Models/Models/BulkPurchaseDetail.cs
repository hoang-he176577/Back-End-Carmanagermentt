using System;
using System.Collections.Generic;

namespace Models.Models;

public partial class BulkPurchaseDetail
{
    public int Id { get; set; }

    public int? PurchaseProposalId { get; set; }

    public int? BranchId { get; set; }

    public int? ProposedQuantity { get; set; }

    public string? BranchNotes { get; set; }

    public decimal? UnitPrice { get; set; }

    public string Status { get; set; } = null!;

    public DateTime? ReceivedDate { get; set; }

    public virtual Branch? Branch { get; set; }

    public virtual PurchaseProposal? PurchaseProposal { get; set; }
}
