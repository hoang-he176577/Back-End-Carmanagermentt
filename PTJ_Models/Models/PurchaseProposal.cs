namespace Models.Models;

public partial class PurchaseProposal
{
    public int Id { get; set; }

    public int? ProposerId { get; set; }

    public int? ManagerId { get; private set; }

    public int? ChiefAccountantId { get; set; } // giữ nguyên vì bạn yêu cầu không đổi entity

    public DateOnly? CreatedDate { get; private set; }

    public DateOnly? ApprovedDate { get; private set; }

    public string? Status { get; private set; }

    public decimal? ProposedCost { get; private set; }

    public string? Description { get; private set; }

    public DateTime? CreatedAt { get; private set; }

    public DateTime? UpdatedAt { get; private set; }

    public DateTime? DeletedAt { get; private set; }

    public virtual ICollection<BulkPurchaseDetail> BulkPurchaseDetails { get; set; } = new List<BulkPurchaseDetail>();

    public virtual User? ChiefAccountant { get; set; }

    public virtual User? Manager { get; set; }

    public virtual User? Proposer { get; set; }
}
