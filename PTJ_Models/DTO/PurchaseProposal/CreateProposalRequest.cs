
namespace Models.DTO.PurchaseProposal
{
    using System.ComponentModel.DataAnnotations;

    public class CreateProposalRequest
    {
        [Required(ErrorMessage = "Please enter proposal description.")]
        [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        [MinLength(5, ErrorMessage = "Description must be at least 5 characters.")]
        public string Description { get; set; } = string.Empty;
    }

}
