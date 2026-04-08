using System;
using System.Collections.Generic;

namespace Models.Models;

public partial class VehicleModel
{
    public int Id { get; set; }

    public string? Manufacturer { get; set; }

    public string? ModelName { get; set; }

    public int? YearFrom { get; set; }

    public int? YearTo { get; set; }

    public int? Seats { get; set; }

    public string? EngineType { get; set; }

    public decimal? DefaultPrice { get; set; }

    public string? EnginePower { get; set; }

    public string? EmissionStandard { get; set; }

    public decimal? PayloadCapacity { get; set; }

    public string? FuelType { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<VehicleAccessoryRequirement> VehicleAccessoryRequirements { get; set; } = new List<VehicleAccessoryRequirement>();

    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
