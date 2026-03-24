using System;

namespace Models.Models;

public partial class BulkPurchaseDetail
{
    public void InitCreate(int branchId, int proposedQuantity, decimal unitPrice, string? branchNotes)
    {
        BranchId = branchId;
        ProposedQuantity = proposedQuantity;
        UnitPrice = unitPrice;
        BranchNotes = branchNotes?.Trim();
        Status = PurchaseProposal.PendingStatus;
        ReceivedDate = null;
    }
}
