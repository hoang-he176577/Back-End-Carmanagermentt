namespace Models.DTO.PurchaseProposal
{
    using System.ComponentModel.DataAnnotations;
    using System.Collections.Generic;

    public class UpdatePurchaseProposalDto
    {
        [Required(ErrorMessage = "Mô tả đề xuất không được để trống.")]
        public string? Description { get; set; }
        public DateTime? CompletionDeadline { get; set; }

        public List<CreateBulkPurchaseDetailDto> Details { get; set; } = new();
    }
}
