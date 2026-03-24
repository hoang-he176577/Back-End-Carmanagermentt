namespace Models.DTO.Drivers;

public sealed class DriverCreateDto
{
    public string Name { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateOnly? HireDate { get; set; }
    public int BranchId { get; set; }
}
