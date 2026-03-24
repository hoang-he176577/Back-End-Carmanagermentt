namespace Models.Models;

public partial class DriverTransferDetail
{
    public int Id { get; set; }
    public int TransferRequestId { get; set; }
    public int DriverId { get; set; }
    public int FromBranchId { get; set; }
    public int ConfirmedByUserId { get; set; }
    public DateTime? TransferDate { get; set; }
    public DateTime? CreatedAt { get; set; }

    public virtual DriverTransferRequest? TransferRequest { get; set; }
    public virtual Driver? Driver { get; set; }
    public virtual Branch? FromBranch { get; set; }
    public virtual User? ConfirmedByUser { get; set; }
}
