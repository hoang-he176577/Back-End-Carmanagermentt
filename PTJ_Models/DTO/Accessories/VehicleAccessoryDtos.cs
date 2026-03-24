using System.ComponentModel.DataAnnotations;

namespace Models.DTO.Accessories;

public class VehicleAccessoryDto
{
    public int Id { get; set; }
    public int? VehicleId { get; set; }
    public string? VehicleLicensePlate { get; set; }
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public int? AccessoryId { get; set; }
    public string? AccessoryCode { get; set; }
    public string? AccessoryName { get; set; }
    public string? AccessoryType { get; set; }
    public int? SourceTransactionId { get; set; }
    public int Quantity { get; set; }
    public string Status { get; set; } = string.Empty;
    public DateOnly? InstallDate { get; set; }
    public DateOnly? RemoveDate { get; set; }
    public string? Notes { get; set; }
    public int? InstalledBy { get; set; }
    public string? InstalledByName { get; set; }
    public int? RemovedBy { get; set; }
    public string? RemovedByName { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class IssueVehicleAccessoryRequestDto
{
    [Range(1, int.MaxValue)]
    public int BranchId { get; set; }

    [Range(1, int.MaxValue)]
    public int VehicleId { get; set; }

    [Range(1, int.MaxValue)]
    public int AccessoryId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    public DateOnly? InstallDate { get; set; }
    public string? Notes { get; set; }
    public int? InstalledBy { get; set; }
}

public class IssueVehicleAccessoryResponseDto
{
    public VehicleAccessoryDto VehicleAccessory { get; set; } = new();
    public int RemainingStock { get; set; }
}

public class ReturnVehicleAccessoryRequestDto
{
    [Required]
    [StringLength(20)]
    public string ActionType { get; set; } = string.Empty;

    public DateOnly? RemoveDate { get; set; }
    public string? Notes { get; set; }
    public int? RemovedBy { get; set; }
}
