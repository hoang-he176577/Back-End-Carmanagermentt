using System;
using System.Collections.Generic;

namespace Models.Models;

public partial class Accessory
{
    public int Id { get; set; }

    public string? Code { get; set; }

    public string? Name { get; set; }

    public int? QuantityInStock { get; set; }

    public decimal? UnitPrice { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<VehicleAccessory> VehicleAccessories { get; set; } = new List<VehicleAccessory>();
}
