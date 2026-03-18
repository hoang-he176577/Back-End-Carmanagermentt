﻿namespace Models.Models;
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

    public virtual Branch? Branch { get; set; }

    public virtual PurchaseProposal? PurchaseProposal { get; set; }

    // =============================
    // METHODS
    // =============================

    public void InitCreate(int branchId, int quantity, decimal unitPrice, string? notes = null)
    {
        if (branchId <= 0)
            throw new Exception("Invalid branchId");

        if (quantity <= 0)
            throw new Exception("Quantity must be > 0");

        if (unitPrice <= 0)
            throw new Exception("Unit price must be > 0");

        BranchId = branchId;
        ProposedQuantity = quantity;
        UnitPrice = unitPrice;
        BranchNotes = notes;
    }

    public void UpdateQuantity(int quantity)
    {
        if (quantity <= 0)
            throw new Exception("Quantity must be > 0");

        ProposedQuantity = quantity;
    }

    public void UpdateUnitPrice(decimal unitPrice)
    {
        if (unitPrice <= 0)
            throw new Exception("Unit price must be > 0");

        UnitPrice = unitPrice;
    }

    public void UpdateNotes(string? notes)
    {
        BranchNotes = notes;
    }

    public decimal GetTotalPrice()
    {
        return ProposedQuantity * UnitPrice;
    }

    public bool IsValid()
    {
        return BranchId > 0
            && ProposedQuantity > 0
            && UnitPrice > 0;
    }
}