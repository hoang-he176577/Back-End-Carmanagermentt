using System;
using System.Collections.Generic;

namespace Models.Models;

public partial class BulkPurchaseDetail
{
    public int Id { get; set; }

    public int? PurchaseProposalId { get; set; }

    public int? BranchId { get; set; }

    public int? ProposedQuantity { get; set; }

    public string? BranchNotes { get; set; }

    public decimal? UnitPrice { get; set; }

    public string? Status { get; set; }

    public DateTime? ReceivedDate { get; set; }

    public int? Seats { get; set; }

    public string? Manufacturer { get; set; }

    public string? AcquisitionMethod { get; set; }

    public string? Version { get; set; }

    public decimal? RegistrationTax { get; set; }

    public decimal? RoadMaintenanceFee { get; set; }

    public decimal? LicensePlateFee { get; set; }

    public decimal? InsuranceFee { get; set; }

    public bool? HasCamera158 { get; set; } = false;

    public bool? HasGsht { get; set; } = false;
    public decimal? FuelNorm { get; set; }

    public virtual Branch? Branch { get; set; }

    public virtual PurchaseProposal? PurchaseProposal { get; set; }
}
