using System;

namespace Models.DTO.PurchaseProposal;

public class PurchaseProposalDto
{
    public int Id { get; set; }
    public string Description { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime CreatedDate { get; set; }
    public decimal ProposedCost { get; set; }

    public List<BulkPurchaseDetailDto> Details { get; set; } = new();
}

public class BulkPurchaseDetailDto
{
    public int Id { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal Price { get; set; }
}
public class PurchaseProposalListDto
{
    public int Id { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
    public DateOnly? CreatedDate { get; set; }
    public decimal? ProposedCost { get; set; }

    public string? ManagerName { get; set; }
}