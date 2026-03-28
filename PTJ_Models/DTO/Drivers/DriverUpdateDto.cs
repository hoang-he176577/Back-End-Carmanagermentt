namespace Models.DTO.Drivers;

public sealed class DriverUpdateDto
{
    public string? Name { get; set; }
    public string? LicenseNumber { get; set; }
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public DateOnly? HireDate { get; set; }
    public int? BranchId { get; set; }
    public string? Status { get; set; }
}
