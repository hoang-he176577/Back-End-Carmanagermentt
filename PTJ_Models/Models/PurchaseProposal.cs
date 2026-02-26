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

    // =============================
    // METHODS
    // =============================

    public void InitCreate( string? description)
    {
        Description = description;
        Status = "Pending";
        CreatedDate = DateOnly.FromDateTime(DateTime.Now);
        CreatedAt = DateTime.Now;
        ProposedCost = 0;
    }

    public void AddDetail(BulkPurchaseDetail detail)
    {
        BulkPurchaseDetails.Add(detail);
        RecalculateCost();
    }

    public void RemoveDetail(BulkPurchaseDetail detail)
    {
        BulkPurchaseDetails.Remove(detail);
        RecalculateCost();
    }

    public void RecalculateCost()
    {
        ProposedCost = BulkPurchaseDetails.Sum(x => x.GetTotalPrice());
        UpdatedAt = DateTime.Now;
    }

    public void ApproveByManager(int managerId)
    {
        if (Status != "Pending")
            throw new Exception("Proposal is not pending");

        ManagerId = managerId;
        Status = "ManagerApproved";
        UpdatedAt = DateTime.Now;
    }

    public void ApproveByChiefAccountant(int accountantId)
    {
        if (Status != "ManagerApproved")
            throw new Exception("Manager must approve first");

        ChiefAccountantId = accountantId;
        Status = "Approved";
        ApprovedDate = DateOnly.FromDateTime(DateTime.Now);
        UpdatedAt = DateTime.Now;
    }

    public void Reject(string reason)
    {
        Status = "Rejected";
        Description += $"\nRejected: {reason}";
        UpdatedAt = DateTime.Now;
    }

    public void UpdateDescription(string description)
    {
        Description = description;
        UpdatedAt = DateTime.Now;
    }

    public void SoftDelete()
    {
        DeletedAt = DateTime.Now;
        Status = "Deleted";
    }

    public bool IsApproved()
    {
        return Status == "Approved";
    }

    public bool IsPending()
    {
        return Status == "Pending";
    }
}
