namespace Models.DTO.VehicleDistribution;

public sealed class TransferPlanUpdateStatusDto
{
    /// <summary>
    /// Allowed values: "Checkout", "Checkin", "Cancelled"
    /// </summary>
    public string? Status { get; set; }
}
