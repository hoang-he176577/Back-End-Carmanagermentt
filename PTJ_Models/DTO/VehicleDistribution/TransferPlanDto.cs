using System;

namespace Models.DTO.VehicleDistribution;

public sealed class TransferPlanDto
{
    public int Id { get; set; }
    public int? VehicleId { get; set; }
    public string? LicensePlate { get; set; }
    public int? FromBranchId { get; set; }
    public string? FromBranchName { get; set; }
    public int? ToBranchId { get; set; }
    public string? ToBranchName { get; set; }
    public int? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    public DateTime? PlannedDepartureDate { get; set; }
    public DateTime? PlannedArrivalDate { get; set; }
    public DateOnly? ExecutedDate { get; set; }
    public string? Status { get; set; }
    public DateTime? CheckoutDate { get; set; }
    public string? CheckoutByName { get; set; }
    public string? CheckoutNote { get; set; }
    public DateTime? CheckinDate { get; set; }
    public string? CheckinByName { get; set; }
    public string? CheckinNote { get; set; }
    public DateTime? CreatedAt { get; set; }
}
