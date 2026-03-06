namespace Models.DTO.VehicleDistribution;

public sealed class BranchStockSummaryDto
{
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public int TotalVehicles { get; set; }
    public int ActiveVehicles { get; set; }
    public int InTransferVehicles { get; set; }
}
