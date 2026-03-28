using System;

namespace Models.DTO.Vehicles;

public sealed class VehicleAssetDto
{
    public int Id { get; set; }
    public string? LicensePlate { get; set; }
    public int? ModelId { get; set; }
    public string? Manufacturer { get; set; }
    public string? ModelName { get; set; }
    public int? YearManufacture { get; set; }
    public DateOnly? PurchaseDate { get; set; }
    public decimal? OriginalCost { get; set; }
    public decimal? CurrentValue { get; set; }
    public decimal? Mileage { get; set; }
    public string? Status { get; set; }
    public int? CurrentBranchId { get; set; }
    public string? CurrentBranchName { get; set; }
    public int? CurrentDriverId { get; set; }
    public string? CurrentDriverName { get; set; }
    public string? ImageUrl { get; set; }

    // --- Vehicle detail fields ---
    public string? Vin { get; set; }
    public string? ChassisNumber { get; set; }
    public string? EngineNumber { get; set; }
    public string? TelematicsImei { get; set; }
    public DateOnly? RegistrationExpirationDate { get; set; }
    public DateOnly? InsuranceExpirationDate { get; set; }
    public string? BadgeType { get; set; }
    public DateOnly? BadgeExpirationDate { get; set; }
    public decimal? FuelNorm { get; set; }

    // --- From VehicleModel ---
    public int? Seats { get; set; }
    public string? EngineType { get; set; }
}
