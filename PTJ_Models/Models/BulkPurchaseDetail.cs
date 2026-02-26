namespace Models.Models;

public partial class BulkPurchaseDetail
{
    public int Id { get; set; }

    public int? PurchaseProposalId { get; set; }

    public int? BranchId { get; set; }

    public int? ProposedQuantity { get; set; }

    public string? BranchNotes { get; set; }

    public virtual Branch? Branch { get; set; }

    public virtual PurchaseProposal? PurchaseProposal { get; set; }

    // =============================
    // METHODS
    // =============================

    public void InitCreate(int branchId, int quantity, string? notes = null)
    {
        if (quantity <= 0)
            throw new Exception("Quantity must be > 0");

        BranchId = branchId;
        ProposedQuantity = quantity;
        BranchNotes = notes;
    }

    public void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new Exception("Quantity must be > 0");

        ProposedQuantity = quantity;
    }

    public void UpdateNotes(string? notes)
    {
        BranchNotes = notes;
    }

    public void AssignProposal(int proposalId)
    {
        PurchaseProposalId = proposalId;
    }

    // Nếu sau này bạn có UnitPrice thì sửa lại
    public decimal GetTotalPrice()
    {
        // hiện tại chưa có price → tạm tính theo quantity
        return ProposedQuantity ?? 0;
    }

    public bool IsValid()
    {
        return BranchId != null && ProposedQuantity > 0;
    }
}
