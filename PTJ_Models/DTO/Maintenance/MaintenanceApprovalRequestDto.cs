using System.ComponentModel.DataAnnotations;

namespace Models.DTO.Maintenance;

public sealed class MaintenanceApprovalRequestDto
{
    [Required]
    [StringLength(20)]
    public string Status { get; set; } = string.Empty; // Approved | Rejected

    public DateOnly? ApprovedDate { get; set; }
}
