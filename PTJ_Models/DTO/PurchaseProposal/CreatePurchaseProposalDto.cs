
namespace Models.DTO.PurchaseProposal
{
    using System.ComponentModel.DataAnnotations;

    public class CreatePurchaseProposalDto
    {

        public string? Description { get; set; }

        public List<CreateBulkPurchaseDetailDto> Details { get; set; } = new();
    }
    public class CreateBulkPurchaseDetailDto
    {
        public int BranchId { get; set; }

        public int Quantity { get; set; }
        public string? Description { get; set; }
        public decimal UnitPrice { get; set; }
        public string? Notes { get; set; }
    }

}
