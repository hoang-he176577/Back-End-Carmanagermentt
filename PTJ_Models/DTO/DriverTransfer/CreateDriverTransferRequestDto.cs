namespace Models.DTO.DriverTransfer;

public sealed class CreateDriverTransferRequestDto
{
    public int RequestedQuantity { get; set; }
    public string? Reason { get; set; }
}
