using System;

namespace Models.DTO.PurchaseProposal;

public class PurchaseProposalDto
{
    public int Id { get; set; }
    public string? Description { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal? ProposedCost { get; set; }
    public string? ProposerName { get; set; } 
    public DateTime? CreatedAt { get; set; }
    public DateTime? CompletionDeadline { get; set; }
    public string? BranchNote { get; set; }
}
public class PurchaseProposalListDto
{
    public int Id { get; set; }
    public string? Description { get; set; }
    public string? Status { get; set; }
    public DateOnly? CreatedDate { get; set; }
    public DateTime? CompletionDeadline { get; set; }
    public decimal? ProposedCost { get; set; }

    public string? ManagerName { get; set; }
}