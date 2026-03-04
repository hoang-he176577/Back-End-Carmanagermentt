using System.ComponentModel.DataAnnotations;

namespace Models.DTO.User;

public sealed class CreateAdminAccountDto
{
    [Required]
    public string? Name { get; set; }

    [Required, EmailAddress]
    public string? Email { get; set; }

    [Required, MinLength(6)]
    public string? Password { get; set; }

    public string? Phone { get; set; }

    public int? BranchId { get; set; }

    [Required]
    public string? Role { get; set; }
}
