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

    public virtual Accessory? Accessory { get; set; }

    public virtual Vehicle? Vehicle { get; set; }
}
