namespace Models.Models;

/// <summary>
/// Bảng đối chiếu xe khi nhận xe từ đề xuất mua
/// </summary>
public partial class VehicleReceptionRecord
{
    public int Id { get; set; }

    // Liên kết đến đề xuất mua
    public int PurchaseProposalId { get; set; }

    // Liên kết đến chi nhánh nhận xe
    public int BranchId { get; set; }

    // Người thực hiện đối chiếu (Operator)
    public int? OperatorId { get; set; }

    // Ngày yêu cầu mua (từ đề xuất)
    public DateOnly? RequestedDate { get; set; }

    // Ngày xe thực tế đã về
    public DateOnly? ReceivedDate { get; set; }

    // Biển số xe
    public string? LicensePlate { get; set; }

    // Số chassis
    public string? ChassisNumber { get; set; }

    // Số máy
    public string? EngineNumber { get; set; }

    // Ảnh chứng minh (VIN, Biển số, ...)
    public string? ReceiptImageUrl { get; set; }

    // Mô tả chi tiết
    public string? Notes { get; set; }

    // Trạng thái: Pending, Completed, Rejected
    public string? Status { get; set; }

    // Thời gian thực hiện
    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    // =============================
    // NAVIGATION PROPERTIES
    // =============================
    public virtual PurchaseProposal? PurchaseProposal { get; set; }

    public virtual Branch? Branch { get; set; }

    public virtual User? Operator { get; set; }

    // =============================
    // STATUS CONSTANTS
    // =============================
    private const string PendingStatus = "Pending";
    private const string CompletedStatus = "Completed";
    private const string RejectedStatus = "Rejected";

    // =============================
    // METHODS
    // =============================

    public void InitCreate(int proposalId, int branchId, int operatorId, DateOnly requestedDate)
    {
        PurchaseProposalId = proposalId;
        BranchId = branchId;
        OperatorId = operatorId;
        RequestedDate = requestedDate;
        Status = PendingStatus;
        CreatedAt = DateTime.Now;
    }

    public void UpdateReceptionDetails(string? licensePlate, string? chassisNumber, string? engineNumber, string? imageUrl, string? notes)
    {
        LicensePlate = licensePlate;
        ChassisNumber = chassisNumber;
        EngineNumber = engineNumber;
        ReceiptImageUrl = imageUrl;
        Notes = notes;
        ReceivedDate = DateOnly.FromDateTime(DateTime.Now);
        UpdatedAt = DateTime.Now;
    }

    public void Complete()
    {
        if (Status != PendingStatus)
            throw new Exception("Record must be in Pending status to complete");

        Status = CompletedStatus;
        UpdatedAt = DateTime.Now;
    }

    public void Reject(string? reason)
    {
        Status = RejectedStatus;
        Notes = reason;
        UpdatedAt = DateTime.Now;
    }

    public bool IsLate()
    {
        if (ReceivedDate.HasValue && RequestedDate.HasValue)
        {
            return ReceivedDate.Value > RequestedDate.Value;
        }
        return false;
    }

    public int GetDaysDelay()
    {
        if (ReceivedDate.HasValue && RequestedDate.HasValue)
        {
            var delay = (ReceivedDate.Value.ToDateTime(TimeOnly.MinValue) - RequestedDate.Value.ToDateTime(TimeOnly.MinValue)).Days;
            return delay > 0 ? delay : 0;
        }
        return 0;
    }
}
