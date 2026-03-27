using System;

namespace Models.DTO.VehicleDistribution;

public sealed class TransferPlanCreateRequestDto
{
    public int? VehicleId { get; set; }
    public int? FromBranchId { get; set; }
    public int? ToBranchId { get; set; }
    public DateTime? PlannedDepartureDate { get; set; }
    public DateTime? PlannedArrivalDate { get; set; }
}
