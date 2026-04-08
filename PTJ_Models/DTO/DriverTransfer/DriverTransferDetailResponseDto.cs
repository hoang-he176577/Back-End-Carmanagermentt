namespace Models.DTO.DriverTransfer;

public sealed class DriverTransferDetailResponseDto
{
    public int Id { get; set; }
    public int DriverId { get; set; }
    public string? DriverName { get; set; }
    public string? DriverLicenseNumber { get; set; }
    public string? DriverPhone { get; set; }
    public int FromBranchId { get; set; }
    public string? FromBranchName { get; set; }
    public int ConfirmedByUserId { get; set; }
    public string? ConfirmedByUserName { get; set; }
    public DateTime? TransferDate { get; set; }
    public DateTime? CreatedAt { get; set; }
}
