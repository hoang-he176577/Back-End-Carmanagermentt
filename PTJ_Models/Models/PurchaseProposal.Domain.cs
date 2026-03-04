using System;
using System.Linq;

namespace Models.Models;

public partial class PurchaseProposal
{
    public void InitCreate(string? description)
    {
        var now = DateTime.Now;
        Description = string.IsNullOrWhiteSpace(description) ? null : description.Trim();
        Status = "Pending";
        CreatedDate = DateOnly.FromDateTime(now);
        CreatedAt = now;
        UpdatedAt = now;
        DeletedAt = null;
        ProposedCost = 0m;
    }

    public void AddDetail(BulkPurchaseDetail detail)
    {
        ArgumentNullException.ThrowIfNull(detail);

        detail.PurchaseProposal = this;
        BulkPurchaseDetails.Add(detail);
        RecalculateProposedCost();
        UpdatedAt = DateTime.Now;
    }

    public void ApproveByManager(int managerId)
    {
        if (managerId <= 0) throw new ArgumentException("ManagerId must be greater than zero.", nameof(managerId));
        if (DeletedAt != null) throw new InvalidOperationException("Cannot approve a deleted proposal.");

        var now = DateTime.Now;
        ManagerId = managerId;
        ApprovedDate = DateOnly.FromDateTime(now);
        Status = "Approved";
        UpdatedAt = now;
    }

    public void ApproveByChiefAccountant(int chiefAccountantId)
    {
        if (chiefAccountantId <= 0) throw new ArgumentException("ChiefAccountantId must be greater than zero.", nameof(chiefAccountantId));
        if (DeletedAt != null) throw new InvalidOperationException("Cannot approve a deleted proposal.");

        ChiefAccountantId = chiefAccountantId;
        UpdatedAt = DateTime.Now;
    }

    public void Reject(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException("Reject reason is required.", nameof(reason));
        if (DeletedAt != null) throw new InvalidOperationException("Cannot reject a deleted proposal.");

        Status = "Rejected";
        UpdatedAt = DateTime.Now;
    }

    public void MarkAsReceived(string licensePlate, int operatorId)
    {
        if (string.IsNullOrWhiteSpace(licensePlate))
            throw new ArgumentException("License plate is required.", nameof(licensePlate));
        if (operatorId <= 0) throw new ArgumentException("OperatorId must be greater than zero.", nameof(operatorId));
        if (DeletedAt != null) throw new InvalidOperationException("Cannot receive a deleted proposal.");

        var now = DateTime.Now;
        foreach (var detail in BulkPurchaseDetails)
        {
            detail.Status = "Received";
            detail.ReceivedDate = now;
        }

        Status = "Received";
        UpdatedAt = now;
    }

    public void MarkAsCompleted()
    {
        if (DeletedAt != null) throw new InvalidOperationException("Cannot complete a deleted proposal.");

        Status = "Completed";
        UpdatedAt = DateTime.Now;
    }

    public void SoftDelete()
    {
        var now = DateTime.Now;
        DeletedAt = now;
        UpdatedAt = now;
        Status = "Deleted";
    }

    private void RecalculateProposedCost()
    {
        ProposedCost = BulkPurchaseDetails.Sum(x => (x.UnitPrice ?? 0m) * (x.ProposedQuantity ?? 0));
    }
}
