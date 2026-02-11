using System.ComponentModel.DataAnnotations;


namespace Models.DTO.Auth
{
    public class LoginResponseDto
    {
        [Required]
        public string AccessToken { get; set; } = string.Empty;  // JWT token chính

        [Required]
        public string RefreshToken { get; set; } = string.Empty; // Token làm mới

        [Required]
        public string TokenType { get; set; } = "Bearer";        // Loại token (thường là Bearer)

        [Range(1, int.MaxValue, ErrorMessage = "ExpiresIn must be greater than 0.")]
        public int ExpiresIn { get; set; }                       // Thời gian sống (giây)

        [Required]
        public UserInfoDto User { get; set; } = new UserInfoDto();
    }
    public class UserInfoDto
    {
        [Required]
        public string Id { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        public List<string> Roles { get; set; } = new();
    }
}
