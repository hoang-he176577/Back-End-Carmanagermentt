using System;

namespace Models.Models;

public partial class BulkPurchaseDetail
{
    public void InitCreate(int branchId, int quantity, decimal unitPrice, string? notes)
    {
        if (branchId <= 0) throw new ArgumentException("BranchId must be greater than zero.", nameof(branchId));
        if (quantity <= 0) throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        if (unitPrice < 0) throw new ArgumentException("UnitPrice cannot be negative.", nameof(unitPrice));

        BranchId = branchId;
        ProposedQuantity = quantity;
        UnitPrice = unitPrice;
        BranchNotes = string.IsNullOrWhiteSpace(notes) ? null : notes.Trim();
        Status = "Pending";
        ReceivedDate = null;
    }
}
