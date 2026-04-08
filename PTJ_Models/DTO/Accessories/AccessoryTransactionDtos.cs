namespace Models.DTO.Accessories;

public class AccessoryTransactionDto
{
    public int Id { get; set; }
    public int AccessoryId { get; set; }
    public string? AccessoryCode { get; set; }
    public string? AccessoryName { get; set; }
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public int? VehicleId { get; set; }
    public string? VehicleLicensePlate { get; set; }
    public int? VehicleAccessoryId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public string? ReferenceType { get; set; }
    public int? ReferenceId { get; set; }
    public int Quantity { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal? UnitPrice { get; set; }
    public string? StockCondition { get; set; }
    public string? Notes { get; set; }
    public int? PerformedBy { get; set; }
    public string? PerformedByName { get; set; }
}
