using System.ComponentModel.DataAnnotations;

namespace Models.DTO.Maintenance;

public sealed class MaintenanceCreateRequestDto
{
    [Required]
    public int? VehicleId { get; set; }

    [Required]
    public DateOnly? RequestDate { get; set; }

    public string? Description { get; set; }

    [Required]
    [Range(0, double.MaxValue)]
    public decimal? EstimatedCost { get; set; }

    [Required]
    [StringLength(20)]
    public string MaintenanceType { get; set; } = string.Empty;
}
