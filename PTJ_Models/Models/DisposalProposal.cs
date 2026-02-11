using System;
using System.Collections.Generic;

namespace Models.Models;

public partial class DisposalProposal
{
    public int Id { get; set; }

    public int? VehicleId { get; set; }

    public int? ProposerId { get; set; }

    public int? ManagerId { get; set; }

    public DateOnly? CreatedDate { get; set; }

    public DateOnly? ApprovedDate { get; set; }

    public string? Status { get; set; }

    public decimal? ProposedPrice { get; set; }

    public string? Reason { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual User? Manager { get; set; }

    public virtual User? Proposer { get; set; }

    public virtual Vehicle? Vehicle { get; set; }
}
