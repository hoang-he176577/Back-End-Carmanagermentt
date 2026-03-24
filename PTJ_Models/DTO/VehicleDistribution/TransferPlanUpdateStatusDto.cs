namespace Models.DTO.VehicleDistribution;

public sealed class TransferPlanUpdateStatusDto
{
    /// <summary>
    /// Allowed values: "Approved", "Rejected", "Executed", "Cancelled"
    /// </summary>
    public string? Status { get; set; }
}
