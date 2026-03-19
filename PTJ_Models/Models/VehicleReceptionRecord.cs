using System.ComponentModel.DataAnnotations.Schema;

namespace Models.Models;

/// <summary>
/// Bảng đối chiếu xe khi nhận xe từ đề xuất mua
/// </summary>
[Table("vehicle_reception_record")]
public partial class VehicleReceptionRecord
{
    [Column("id")]
    public int Id { get; set; }

    // Liên kết đến đề xuất mua
    [Column("purchase_proposal_id")]
    public int PurchaseProposalId { get; set; }

    // Liên kết đến chi nhánh nhận xe
    [Column("branch_id")]
    public int BranchId { get; set; }

    // Người thực hiện đối chiếu (Operator)
    [Column("operator_id")]
    public int? OperatorId { get; set; }

    // Ngày yêu cầu mua (từ đề xuất)
    [Column("requested_date")]
    public DateOnly? RequestedDate { get; set; }

    // Ngày nhận xe thực tế
    [Column("received_date")]
    public DateOnly? ReceivedDate { get; set; }

    // Biển số xe
    [Column("license_plate")]
    public string? LicensePlate { get; set; }

    // Số chassis
    [Column("chassis_number")]
    public string? ChassisNumber { get; set; }

    // Số máy
    [Column("engine_number")]
    public string? EngineNumber { get; set; }

    // Ảnh chứng minh (VIN, Biển số, ...)
    [Column("receipt_image_url")]
    public string? ReceiptImageUrl { get; set; }

    // Mô tả chi tiết
    [Column("notes")]
    public string? Notes { get; set; }

    // Trạng thái: Pending, Completed, Rejected
    [Column("status")]
    public string? Status { get; set; }

    [Column("reason")]
    public string? Reason { get; set; } // Map với SQL Script (Lý do từ chối)

    [Column("created_at")]
    public DateTime? CreatedAt { get; set; }

    [Column("updated_at")]
    public DateTime? UpdatedAt { get; set; }

    [Column("deleted_at")]
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
    public const string ReceivedPendingPaymentStatus = "Received_Pending_Payment";
    public const string CompletedStatus = "Completed";
    public const string RejectedStatus = "Rejected";

    // =============================
    // METHODS
    // =============================

    public void InitCreate(int proposalId, int branchId, int operatorId, DateOnly requestedDate)
    {
        PurchaseProposalId = proposalId;
        BranchId = branchId;
        OperatorId = operatorId;
        RequestedDate = requestedDate;
        Status = ReceivedPendingPaymentStatus;
        CreatedAt = DateTime.Now;
    }

    public void UpdateReceptionDetails(string? licensePlate, string? chassisNumber, string? engineNumber, string? imageUrl, string? notes)
    {
        LicensePlate = licensePlate;
        ChassisNumber = chassisNumber;
        EngineNumber = engineNumber;
        ReceiptImageUrl = imageUrl;
        Notes = notes;
        UpdatedAt = DateTime.Now;
    }

    public void Complete()
    {
        if (Status != ReceivedPendingPaymentStatus)
            throw new Exception("Record must be in Received_Pending_Payment status to complete");

        Status = CompletedStatus;
        ReceivedDate = DateOnly.FromDateTime(DateTime.Now);
        UpdatedAt = DateTime.Now;
    }

    public void Reject(string? notes)
    {
        Status = RejectedStatus;
        if (!string.IsNullOrEmpty(notes))
        {
            Notes = string.IsNullOrEmpty(Notes) ? notes : $"{Notes}\n{notes}";
        }
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
        if (ReceivedDate.HasValue && RequestedDate.HasValue && IsLate())
        {
            return (ReceivedDate.Value.DayNumber - RequestedDate.Value.DayNumber);
        }
        return 0;
    }
}
