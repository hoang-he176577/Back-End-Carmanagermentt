namespace Models.DTO.Maintenance;

public sealed class MaintenanceRequestDto
{
    public int Id { get; set; }
    public int? VehicleId { get; set; }
    public string? VehicleLicensePlate { get; set; }
    public string? VehicleModelName { get; set; }
    public int? OperatorId { get; set; }
    public DateOnly? RequestDate { get; set; }
    public string? Description { get; set; }
    public decimal? EstimatedCost { get; set; }
    public string? Status { get; set; }
    public string MaintenanceType { get; set; } = string.Empty;
    public int? AccountantId { get; set; }
    public string? ApproverName { get; set; }
    public DateOnly? ApprovedDate { get; set; }
    public decimal? ActualCost { get; set; }
    public DateOnly? CompletionDate { get; set; }
    public string? CompletionNote { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}
