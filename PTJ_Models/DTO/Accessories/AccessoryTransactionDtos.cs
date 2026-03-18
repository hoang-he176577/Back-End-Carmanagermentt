namespace Models.DTO.Accessories;

public class AccessoryTransactionDto
{
    public int Id { get; set; }
    public int AccessoryId { get; set; }
    public int? VehicleId { get; set; }
    public int? VehicleAccessoryId { get; set; }
    public string TransactionType { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public DateTime TransactionDate { get; set; }
    public decimal? UnitPrice { get; set; }
    public string? Notes { get; set; }
    public int? PerformedBy { get; set; }
}
