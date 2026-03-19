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



    // =============================
    // STATUS CONSTANTS
    // =============================

    private const string PendingStatus = "Pending";
    private const string ApprovedStatus = "Approved";
    private const string RejectedStatus = "Rejected";
    private const string DeletedStatus = "Deleted";

        // =============================
    // METHODS
    // =============================

    public void InitCreate(string? description, int proposerId) 
    {
        Description = description;
        Status = PendingStatus;
        CreatedDate = DateOnly.FromDateTime(DateTime.Now);
        CreatedAt = DateTime.Now;
        ProposedCost = 0;
        ProposerId = proposerId; 
    }

    public void AddDetail(BulkPurchaseDetail detail)
    {
        if (!IsPending())
            throw new Exception("Cannot modify proposal when not pending");

        if (detail == null)
            throw new Exception("Detail is required");

        if (!detail.IsValid())
            throw new Exception("Invalid detail");

        BulkPurchaseDetails.Add(detail);

        RecalculateCost();
    }

    public void RemoveDetail(BulkPurchaseDetail detail)
    {
        if (!IsPending())
            throw new Exception("Cannot modify proposal when not pending");

        BulkPurchaseDetails.Remove(detail);

        RecalculateCost();
    }

    public void RecalculateCost()
    {
        ProposedCost = BulkPurchaseDetails.Sum(x => x.GetTotalPrice());
        UpdatedAt = DateTime.Now;
    }

    // ✅ CHỈ MANAGER APPROVE
    public void ApproveByManager(int managerId)
    {
        if (Status != PendingStatus)
            throw new Exception("Proposal is not pending");

        if (managerId <= 0)
            throw new Exception("Invalid managerId");

        ManagerId = managerId;
        Status = ApprovedStatus;
        ApprovedDate = DateOnly.FromDateTime(DateTime.Now);
        UpdatedAt = DateTime.Now;
    }

    public void Reject(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new Exception("Reject reason is required");

        if (Status == ApprovedStatus)
            throw new Exception("Cannot reject approved proposal");

        Status = RejectedStatus;
        Description = (Description ?? "") + $"\nRejected: {reason}";
        UpdatedAt = DateTime.Now;
    }

    public void UpdateDescription(string description)
    {
        if (!IsPending())
            throw new Exception("Cannot update description when not pending");

        Description = description;
        UpdatedAt = DateTime.Now;
    }

    public void SoftDelete()
    {
        if (Status == ApprovedStatus)
            throw new Exception("Cannot delete approved proposal");

        DeletedAt = DateTime.Now;
        Status = DeletedStatus;
        UpdatedAt = DateTime.Now;
    }

    public bool IsApproved()
    {
        return Status == ApprovedStatus;
    }

    public bool IsPending()
    {
        return Status == PendingStatus;
    }

    public void ApproveByChiefAccountant(int accountantId)
    {
        if (accountantId <= 0)
            throw new Exception("Invalid accountantId");

        ChiefAccountantId = accountantId;
        UpdatedAt = DateTime.Now;
    }

    public void MarkAsReceived(string licensePlate, int operatorId)
    {
        if (Status != ApprovedStatus)
            throw new Exception("Proposal must be approved before reception.");

        Status = "Received_Pending_Payment";
        Description = (Description ?? "") + $"\n[Reception Confirmed]: License Plate {licensePlate} by User {operatorId}";
        UpdatedAt = DateTime.Now;
    }

    public void MarkAsCompleted()
    {
        Status = "Completed";
        UpdatedAt = DateTime.Now;
    }

    public void ConfirmReceipt(string notes)
    {
        Description = (Description ?? "") + $"\n[Xác nhận từ chi nhánh]: {notes} vào ngày {DateTime.Now}";
        Status = "Completed";
        UpdatedAt = DateTime.Now;
    }

    public void RevertToApproved(string reason)
    {
        Status = ApprovedStatus;
        Description = (Description ?? "") + $"\n[Hoàn tác đối chiếu]: {reason}";
        UpdatedAt = DateTime.Now;
    }
}

