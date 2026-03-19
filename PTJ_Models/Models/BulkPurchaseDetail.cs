
namespace Models.Models;
using System.ComponentModel.DataAnnotations.Schema;
public partial class BulkPurchaseDetail
{
    public int Id { get; set; }

    public int PurchaseProposalId { get; private set; }

    public int BranchId { get; private set; }

    public int ProposedQuantity { get; private set; }

    [Column("unit_price")]
    public decimal UnitPrice { get; set; }

    [Column("seats")]
    public int? Seats { get; set; }

    [Column("manufacturer")]
    public string? Manufacturer { get; set; }

    public string? BranchNotes { get; private set; }

    [Column("status")]
    public string? Status { get; set; }

    [Column("received_date")]
    public DateTime? ReceivedDate { get; set; }

    public virtual Branch? Branch { get; set; }

    public virtual PurchaseProposal? PurchaseProposal { get; set; }
}