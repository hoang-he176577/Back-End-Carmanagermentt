using System.ComponentModel.DataAnnotations;

namespace Models.DTO.User
{
    public sealed class AdminCreateUserRequestDto
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(100, ErrorMessage = "Email must not exceed 100 characters.")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [StringLength(100, ErrorMessage = "Password must not exceed 100 characters.")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Name is required.")]
        [StringLength(100, ErrorMessage = "Name must not exceed 100 characters.")]
        public string Name { get; set; } = string.Empty;

        [StringLength(20, ErrorMessage = "Phone must not exceed 20 characters.")]
        public string? Phone { get; set; }

        public int? BranchId { get; set; }

        [Required(ErrorMessage = "Role is required.")]
        [StringLength(100, ErrorMessage = "Role must not exceed 100 characters.")]
        public string Role { get; set; } = string.Empty;
    }
}
