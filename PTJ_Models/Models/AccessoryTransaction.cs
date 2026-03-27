using System;
using System.Collections.Generic;

namespace Models.Models;

public partial class AccessoryTransaction
{
    public int Id { get; set; }

    public int AccessoryId { get; set; }

    public int? VehicleId { get; set; }

    public int? VehicleAccessoryId { get; set; }

    public string TransactionType { get; set; } = null!;

    public int Quantity { get; set; }

    public DateTime TransactionDate { get; set; }

    public decimal? UnitPrice { get; set; }

    public string? Notes { get; set; }

    public int? PerformedBy { get; set; }

    public int? BranchId { get; set; }

    public string? ReferenceType { get; set; }

    public int? ReferenceId { get; set; }

    public string? StockCondition { get; set; }

    public virtual Accessory Accessory { get; set; } = null!;

    public virtual Branch? Branch { get; set; }

    public virtual User? PerformedByNavigation { get; set; }

    public virtual Vehicle? Vehicle { get; set; }

    public virtual ICollection<VehicleAccessory> VehicleAccessories { get; set; } = new List<VehicleAccessory>();

    public virtual VehicleAccessory? VehicleAccessory { get; set; }
}
