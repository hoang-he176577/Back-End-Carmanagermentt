using System;
using System.Collections.Generic;

namespace Models.Models;

public partial class VehicleAccessory
{
    public int Id { get; set; }

    public int? VehicleId { get; set; }

    public int? AccessoryId { get; set; }

    public DateOnly? InstallDate { get; set; }

    public DateOnly? RemoveDate { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public int Quantity { get; set; }

    public string Status { get; set; } = null!;

    public int? InstalledBy { get; set; }

    public int? RemovedBy { get; set; }

    public int? BranchId { get; set; }

    public int? SourceTransactionId { get; set; }

    public virtual Accessory? Accessory { get; set; }

    public virtual ICollection<AccessoryTransaction> AccessoryTransactions { get; set; } = new List<AccessoryTransaction>();

    public virtual Branch? Branch { get; set; }

    public virtual User? InstalledByNavigation { get; set; }

    public virtual User? RemovedByNavigation { get; set; }

    public virtual AccessoryTransaction? SourceTransaction { get; set; }

    public virtual Vehicle? Vehicle { get; set; }
}
