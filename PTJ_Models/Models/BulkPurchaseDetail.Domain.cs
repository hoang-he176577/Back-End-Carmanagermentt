namespace Models.Models;

public partial class BulkPurchaseDetail
{
    public void InitCreate(int branchId, int quantity, decimal unitPrice, string? notes = null)
    {
        if (branchId <= 0)
            throw new Exception("Invalid branchId");

        if (quantity <= 0)
            throw new Exception("Quantity must be > 0");

        if (unitPrice <= 0)
            throw new Exception("Unit price must be > 0");

        BranchId = branchId;
        ProposedQuantity = quantity;
        UnitPrice = unitPrice;
        BranchNotes = notes;
    }

    public void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new Exception("Quantity must be > 0");

        ProposedQuantity = quantity;
    }

    public void UpdateUnitPrice(decimal unitPrice)
    {
        if (unitPrice <= 0)
            throw new Exception("Unit price must be > 0");

        UnitPrice = unitPrice;
    }

    public void UpdateNotes(string? notes)
    {
        BranchNotes = notes;
    }

    public decimal GetTotalPrice()
    {
        return (ProposedQuantity ?? 0) * (UnitPrice ?? 0);
    }

    public bool IsValid()
    {
        return (BranchId ?? 0) > 0
            && (ProposedQuantity ?? 0) > 0
            && (UnitPrice ?? 0) > 0;
    }
}
