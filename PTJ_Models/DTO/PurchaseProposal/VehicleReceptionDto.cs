namespace Models.DTO.PurchaseProposal;

/// <summary>
/// DTO để gửi yêu cầu đối chiếu xe từ frontend
/// </summary>
public class CreateVehicleReceptionDto
{
    public int PurchaseProposalId { get; set; }

    public int BranchId { get; set; }

    public string? LicensePlate { get; set; }

    public string? ChassisNumber { get; set; }

    public string? EngineNumber { get; set; }

    /// <summary>
    /// Ảnh base64 hoặc URL sau khi upload
    /// </summary>
    public string? ReceiptImageUrl { get; set; }

    public string? Notes { get; set; }
}

/// <summary>
/// DTO để cập nhật trạng thái (Complete/Reject)
/// </summary>
public class UpdateVehicleReceptionStatusDto
{
    public string Status { get; set; } = string.Empty; // Completed, Rejected

    public string? Reason { get; set; } // Lý do từ chối
}

/// <summary>
/// DTO trả về danh sách kế hoạch mua
/// </summary>
public class PurchasePlanDto
{
    public int ProposalId { get; set; }

    public string? Description { get; set; }

    public string? Status { get; set; }

    public DateOnly? CreatedDate { get; set; }

    public DateOnly? ApprovedDate { get; set; }

    public decimal? ProposedCost { get; set; }

    public string? ManagerName { get; set; }

    /// <summary>
    /// Danh sách chi nhánh + số lượng trong đề xuất này
    /// </summary>
    public List<BranchPurchaseDetailDto>? BranchDetails { get; set; }

    /// <summary>
    /// Ưu tiên (1=cao, 3=thấp) - dùng để sắp xếp
    /// </summary>
    public int Priority { get; set; } = 3;
}

public class BranchPurchaseDetailDto
{
    public int BranchId { get; set; }

    public string? BranchName { get; set; }

    public int ProposedQuantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TotalPrice => ProposedQuantity * UnitPrice;

    public string? BranchNotes { get; set; }

    /// <summary>
    /// Số lượng xe đã nhận
    /// </summary>
    public int ReceivedQuantity { get; set; } = 0;

    /// <summary>
    /// Ngày yêu cầu
    /// </summary>
    public DateOnly? RequestedDate { get; set; }
}
