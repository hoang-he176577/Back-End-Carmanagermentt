namespace Models.DTO.Vehicles;

public sealed class VehicleAssignRequestDto
{
    public int DriverId { get; set; }
    public string? AssignDate { get; set; }
    public string? Notes { get; set; }
}
