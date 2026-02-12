using System.ComponentModel.DataAnnotations;

namespace Models.DTO.Auth
{
    public class ResendVerificationRequestDto
    {
        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        [StringLength(100, ErrorMessage = "Email must not exceed 100 characters.")]
        public string Email { get; set; } = string.Empty;
    }
}
