namespace Models.DTO.DriverTransfer;

public sealed class DriverTransferRequestResponseDto
{
    public int Id { get; set; }
    public int RequestingBranchId { get; set; }
    public string? RequestingBranchName { get; set; }
    public int RequestedQuantity { get; set; }
    public int FulfilledQuantity { get; set; }
    public int RemainingQuantity => RequestedQuantity - FulfilledQuantity;
    public string? Status { get; set; }
    public string? Reason { get; set; }
    public int CreatedByUserId { get; set; }
    public string? CreatedByUserName { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<DriverTransferDetailResponseDto> TransferDetails { get; set; } = new();
}
