using System;
using System.Linq;

namespace Models.Models;

public partial class PurchaseProposal
{
    public const string PendingStatus = "Pending";
    public const string ManagerApprovedStatus = "ManagerApproved";
    public const string ApprovedStatus = "Approved";
    public const string RejectedStatus = "Rejected";
    public const string CompletedStatus = "Completed";

    public void InitCreate(string description, int proposerId, DateTime? completionDeadline)
    {
        Description = description?.Trim();
        ProposerId = proposerId;
        CompletionDeadline = completionDeadline;
        Status = PendingStatus;
        CreatedDate = DateOnly.FromDateTime(DateTime.Now);
        CreatedAt = DateTime.Now;
        UpdatedAt = DateTime.Now;
        ProposedCost = 0;
    }

    public void AddDetail(BulkPurchaseDetail detail)
    {
        if (detail == null)
        {
            throw new ArgumentNullException(nameof(detail));
        }

        BulkPurchaseDetails.Add(detail);
        detail.PurchaseProposal = this;
        RecalculateProposedCost();
        UpdatedAt = DateTime.Now;
    }

    public void ApproveByManager(int managerId)
    {
        ManagerId = managerId;
        Status = ManagerApprovedStatus;
        UpdatedAt = DateTime.Now;
    }

    public void ApproveByChiefAccountant(int accountantId)
    {
        ChiefAccountantId = accountantId;
        ApprovedDate ??= DateOnly.FromDateTime(DateTime.Now);
        if (!string.Equals(Status, CompletedStatus, StringComparison.OrdinalIgnoreCase))
        {
            Status = ApprovedStatus;
        }

        UpdatedAt = DateTime.Now;
    }

    public void Reject(string? reason)
    {
        Status = RejectedStatus;
        UpdatedAt = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(reason))
        {
            Description = string.IsNullOrWhiteSpace(Description)
                ? $"Rejected: {reason.Trim()}"
                : $"{Description}{Environment.NewLine}Rejected: {reason.Trim()}";
        }
    }

    public void SoftDelete()
    {
        DeletedAt = DateTime.Now;
        UpdatedAt = DateTime.Now;
    }

    public void MarkAsReceived(string? licensePlate, int operatorId)
    {
        Status = VehicleReceptionRecord.ReceivedPendingPaymentStatus;
        UpdatedAt = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(licensePlate))
        {
            Description = string.IsNullOrWhiteSpace(Description)
                ? $"Received vehicle: {licensePlate.Trim()}"
                : Description;
        }
    }

    public void MarkAsCompleted()
    {
        Status = CompletedStatus;
        UpdatedAt = DateTime.Now;
    }

    public void RevertToApproved(string? reason)
    {
        Status = ApprovedStatus;
        UpdatedAt = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(reason))
        {
            Description = string.IsNullOrWhiteSpace(Description)
                ? $"Rollback reason: {reason.Trim()}"
                : $"{Description}{Environment.NewLine}Rollback reason: {reason.Trim()}";
        }
    }

    public void ConfirmReceipt(string? notes)
    {
        Status = VehicleReceptionRecord.ReceivedPendingPaymentStatus;
        UpdatedAt = DateTime.Now;
        if (!string.IsNullOrWhiteSpace(notes))
        {
            Description = string.IsNullOrWhiteSpace(Description)
                ? $"Receipt confirmed: {notes.Trim()}"
                : $"{Description}{Environment.NewLine}Receipt confirmed: {notes.Trim()}";
        }
    }

    private void RecalculateProposedCost()
    {
        ProposedCost = BulkPurchaseDetails.Sum(detail =>
        {
            var unitCost = (detail.UnitPrice ?? 0) +
                           (detail.RegistrationTax ?? 0) +
                           (detail.RoadMaintenanceFee ?? 0) +
                           (detail.LicensePlateFee ?? 0) +
                           (detail.InsuranceFee ?? 0);
            return (detail.ProposedQuantity ?? 0) * unitCost;
        });
    }
}
