namespace Models.DTO.Drivers;

public sealed class DriverDropdownDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? LicenseNumber { get; set; }
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public bool IsAssigned { get; set; }
}
