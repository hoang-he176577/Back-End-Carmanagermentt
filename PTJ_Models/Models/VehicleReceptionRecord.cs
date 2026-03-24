using System;
using System.Collections.Generic;

namespace Models.Models;

public partial class VehicleReceptionRecord
{
    public int Id { get; set; }

    public int PurchaseProposalId { get; set; }

    public int BranchId { get; set; }

    public int? OperatorId { get; set; }

    public DateOnly? RequestedDate { get; set; }

    public DateOnly? ReceivedDate { get; set; }

    public string? LicensePlate { get; set; }

    public string? ChassisNumber { get; set; }

    public string? EngineNumber { get; set; }

    public string? ReceiptImageUrl { get; set; }

    public string? Notes { get; set; }

    public string? Status { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public string? Reason { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual User? Operator { get; set; }

    public virtual PurchaseProposal PurchaseProposal { get; set; } = null!;
}
