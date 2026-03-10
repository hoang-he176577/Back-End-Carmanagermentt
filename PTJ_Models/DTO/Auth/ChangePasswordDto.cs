using System.ComponentModel.DataAnnotations;

namespace Models.DTO.Auth
{
    public class RequestChangePasswordDto
    {
        [Required(ErrorMessage = "Current password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [StringLength(100, ErrorMessage = "Password must not exceed 100 characters.")]
        public string CurrentPassword { get; set; } = string.Empty;
    }

    public class ConfirmChangePasswordDto
    {
        [Required(ErrorMessage = "Token is required.")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "New password is required.")]
        [MinLength(6, ErrorMessage = "Password must be at least 6 characters.")]
        [StringLength(100, ErrorMessage = "Password must not exceed 100 characters.")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Confirm password is required.")]
        [Compare(nameof(NewPassword), ErrorMessage = "Confirm password does not match.")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
