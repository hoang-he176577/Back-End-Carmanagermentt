using System;
using System.Collections.Generic;

namespace Models.Models;

public partial class VehicleAccessoryRequirement
{
    public int Id { get; set; }

    public int ModelId { get; set; }

    public int AccessoryId { get; set; }

    public int RequiredQuantity { get; set; }

    public bool IsMandatory { get; set; }

    public string? Notes { get; set; }

    public virtual Accessory Accessory { get; set; } = null!;

    public virtual VehicleModel Model { get; set; } = null!;
}
