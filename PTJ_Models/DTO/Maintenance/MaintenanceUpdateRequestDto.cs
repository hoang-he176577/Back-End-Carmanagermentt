using System.ComponentModel.DataAnnotations;

namespace Models.DTO.Maintenance;

public sealed class MaintenanceUpdateRequestDto
{
    public int? VehicleId { get; set; }
    public DateOnly? RequestDate { get; set; }
    public string? Description { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? EstimatedCost { get; set; }

    [StringLength(20)]
    public string? MaintenanceType { get; set; }

    [StringLength(20)]
    public string? Status { get; set; }

    public int? AccountantId { get; set; }
    public DateOnly? ApprovedDate { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? ActualCost { get; set; }

    public DateOnly? CompletionDate { get; set; }

    [StringLength(1000)]
    public string? CompletionNote { get; set; }
}
