namespace Models.DTO.PurchaseProposal;

/// <summary>
/// DTO để gửi yêu cầu đối chiếu xe từ frontend
/// </summary>
public class CreateVehicleReceptionDto
{
    public int PurchaseProposalId { get; set; }

    public int BranchId { get; set; }

    public string? LicensePlate { get; set; }

    public string? Version { get; set; }

    public string? ChassisNumber { get; set; }

    public string? EngineNumber { get; set; }

    public string? Vin { get; set; }

    public string? TelematicsImei { get; set; }

    public DateOnly? RegistrationExpirationDate { get; set; }

    public DateOnly? InsuranceExpirationDate { get; set; }

    public string? BadgeType { get; set; }

    public DateOnly? BadgeExpirationDate { get; set; }

    public decimal? FuelNorm { get; set; }

    /// <summary>
    /// Ảnh base64 hoặc URL sau khi upload
    /// </summary>
    public string? ReceiptImageUrl { get; set; }

    public string? Notes { get; set; }
    public int? YearManufacture { get; set; }
    public decimal? Mileage { get; set; }
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
    public DateTime? CompletionDeadline { get; set; }

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

    /// <summary>
    /// Danh sách xe đã tiếp nhận thực tế
    /// </summary>
    public List<VehicleReceptionRecordDto>? Receptions { get; set; }
}

public class BranchPurchaseDetailDto
{
    public int Id { get; set; }
    public string? ProposerBranchName { get; set; }
    public int BranchId { get; set; }

    public string? BranchName { get; set; }

    public int ProposedQuantity { get; set; }

    public decimal UnitPrice { get; set; }
    public decimal? FuelNorm { get; set; }
    public int? Seats { get; set; }

    public string? Manufacturer { get; set; }

    public string? Version { get; set; }

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

    // Các trường TCO & NĐ158 (Phase 2)
    public decimal? RegistrationTax { get; set; }
    public decimal? RoadMaintenanceFee { get; set; }
    public decimal? LicensePlateFee { get; set; }
    public decimal? InsuranceFee { get; set; }
    public bool HasCamera158 { get; set; }
    public bool HasGsht { get; set; }
    public string? AcquisitionMethod { get; set; }
    public DateTime? CompletionDeadline { get; set; }
}
