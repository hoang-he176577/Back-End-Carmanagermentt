namespace Models.Models;

public partial class VehicleReceptionRecord
{
    public const string PendingStatus = "Pending";
    public const string CompletedStatus = "Completed";
    public const string RejectedStatus = "Rejected";
    public const string ReceivedPendingPaymentStatus = "Received_Pending_Payment";

    public void InitCreate(int purchaseProposalId, int branchId, int operatorId, DateOnly requestedDate)
    {
        PurchaseProposalId = purchaseProposalId;
        BranchId = branchId;
        OperatorId = operatorId;
        RequestedDate = requestedDate;
        Status = PendingStatus;
        CreatedAt = DateTime.Now;
        UpdatedAt = DateTime.Now;
        DeletedAt = null;
    }

    public void UpdateReceptionDetails(string? licensePlate, string? chassisNumber, string? engineNumber, string? receiptImageUrl, string? notes)
    {
        LicensePlate = licensePlate?.Trim();
        ChassisNumber = chassisNumber?.Trim();
        EngineNumber = engineNumber?.Trim();
        ReceiptImageUrl = receiptImageUrl?.Trim();
        Notes = notes?.Trim();
        UpdatedAt = DateTime.Now;
    }

    public void Complete()
    {
        Status = CompletedStatus;
        ReceivedDate ??= DateOnly.FromDateTime(DateTime.Now);
        UpdatedAt = DateTime.Now;
    }

    public void Reject(string? reason)
    {
        Status = RejectedStatus;
        Reason = reason?.Trim();
        UpdatedAt = DateTime.Now;
    }

    public int GetDaysDelay()
    {
        if (!RequestedDate.HasValue)
        {
            return 0;
        }

        var comparisonDate = ReceivedDate ?? DateOnly.FromDateTime(DateTime.Now);
        return comparisonDate.DayNumber - RequestedDate.Value.DayNumber;
    }

    public bool IsLate() => GetDaysDelay() > 0;
}
