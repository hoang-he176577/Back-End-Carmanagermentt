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

    public virtual Accessory Accessory { get; set; } = null!;

    public virtual User? PerformedByNavigation { get; set; }

    public virtual Vehicle? Vehicle { get; set; }

    public virtual VehicleAccessory? VehicleAccessory { get; set; }
}
