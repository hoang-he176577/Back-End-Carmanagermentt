namespace Models.DTO.PurchaseProposal;

/// <summary>
/// DTO trả về thông tin bản ghi đối chiếu xe
/// </summary>
public class VehicleReceptionRecordDto
{
    public int Id { get; set; }

    public int ProposalId { get; set; }

    public int BranchId { get; set; }

    public string? BranchName { get; set; }

    public int? OperatorId { get; set; }

    public string? OperatorName { get; set; }

    public DateOnly? RequestedDate { get; set; }

    public DateOnly? ReceivedDate { get; set; }

    public string? LicensePlate { get; set; }

    public string? ChassisNumber { get; set; }

    public string? EngineNumber { get; set; }

    public string? ReceiptImageUrl { get; set; }

    public string? Notes { get; set; }

    public string? Status { get; set; }

    /// <summary>
    /// Số ngày trễ hạn (âm = sớm, dương = trễ)
    /// </summary>
    public int DaysDelay { get; set; } = 0;

    /// <summary>
    /// Có trễ hay không
    /// </summary>
    public bool IsLate { get; set; } = false;

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }
}
