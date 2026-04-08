using System;
using System.ComponentModel.DataAnnotations;

namespace Models.DTO.Vehicles;

public sealed class VehicleCreateRequestDto
{
    [Required]
    public string? LicensePlate { get; set; }

    [Required]
    public int? ModelId { get; set; }

    public int? YearManufacture { get; set; }

    public DateOnly? PurchaseDate { get; set; }

    public decimal? OriginalCost { get; set; }

    public decimal? CurrentValue { get; set; }

    public decimal? Mileage { get; set; }

    public string? Status { get; set; }

    public int? CurrentDriverId { get; set; }
}
