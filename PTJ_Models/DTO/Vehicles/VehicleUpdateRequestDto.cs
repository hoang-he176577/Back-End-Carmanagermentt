using System;

namespace Models.DTO.Vehicles;

public sealed class VehicleUpdateRequestDto
{
    public string? LicensePlate { get; set; }
    public int? ModelId { get; set; }
    public int? YearManufacture { get; set; }
    public DateOnly? PurchaseDate { get; set; }
    public decimal? OriginalCost { get; set; }
    public decimal? CurrentValue { get; set; }
    public decimal? Mileage { get; set; }
    public string? Status { get; set; }
    public int? CurrentDriverId { get; set; }

    // --- Editable document/badge fields ---
    public string? BadgeType { get; set; }
    public DateOnly? BadgeExpirationDate { get; set; }
    public DateOnly? RegistrationExpirationDate { get; set; }
    public DateOnly? InsuranceExpirationDate { get; set; }
}
