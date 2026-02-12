using System.ComponentModel.DataAnnotations;

namespace Models.DTO.Auth
{
    public class RefreshRequestDto
    {
        [Required(ErrorMessage = "Refresh token is required.")]
        public string RefreshToken { get; set; } = string.Empty;

        [StringLength(250, ErrorMessage = "DeviceInfo must not exceed 250 characters.")]
        public string? DeviceInfo { get; set; }
    }
}
