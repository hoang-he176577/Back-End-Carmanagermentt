using System.ComponentModel.DataAnnotations;

namespace Models.DTO.Auth
{
    public class VerifyEmailRequestDto
    {
        [Required(ErrorMessage = "Token is required.")]
        public string Token { get; set; } = string.Empty;
    }
}
