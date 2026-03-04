using System.ComponentModel.DataAnnotations;

namespace Models.DTO.User
{
    public sealed class AdminUpdateUserStatusRequestDto
    {
        [Required(ErrorMessage = "IsActive is required.")]
        public bool IsActive { get; set; }
    }
}
