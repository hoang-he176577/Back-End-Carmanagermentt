namespace Models.DTO.User;

public sealed class AdminAccountDto
{
    public int Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Phone { get; set; }
    public int? BranchId { get; set; }
    public string? BranchName { get; set; }
    public List<string> Roles { get; set; } = new();
    public bool? EmailVerified { get; set; }
    public bool IsActive { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? LastLogin { get; set; }
    public string? Warning { get; set; }
}
