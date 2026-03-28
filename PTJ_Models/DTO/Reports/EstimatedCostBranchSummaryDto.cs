namespace Models.DTO.Reports;

public sealed class EstimatedCostBranchSummaryDto
{
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public decimal MaintenanceCost { get; set; }
    public decimal VehiclePurchaseCost { get; set; }
    public decimal AccessoryPurchaseCost { get; set; }
    public decimal DisposalCost { get; set; }
    public decimal TotalCost { get; set; }
}
