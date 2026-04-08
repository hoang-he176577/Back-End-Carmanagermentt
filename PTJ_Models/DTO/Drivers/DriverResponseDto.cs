namespace Models.DTO.Drivers;

public sealed class DriverResponseDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? LicenseNumber { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public DateOnly? HireDate { get; set; }
    public string? Status { get; set; }
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public int? CurrentVehicleId { get; set; }
    public string? CurrentVehicleLicensePlate { get; set; }
    public string? CurrentVehicleModelName { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
