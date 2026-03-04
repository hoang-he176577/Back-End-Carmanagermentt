using System.ComponentModel.DataAnnotations;

namespace Models.DTO.Vehicles;

/// <summary>
/// Extended vehicle creation DTO used by the Accountant "Asset Create" form.
/// Contains core Vehicle fields plus optional registration/insurance/identification data.
/// Extra fields (vin, engineNumber, etc.) are accepted by the API but currently 
/// stored only if the schema supports them; otherwise they are silently ignored.
/// </summary>
public sealed class AssetCreateRequestDto
{
    // ── Core vehicle fields ──
    [Required]
    public string? LicensePlate { get; set; }

    [Required]
    public int? ModelId { get; set; }

    public int? YearManufacture { get; set; }
    public string? PurchaseDate { get; set; }
    public decimal? OriginalCost { get; set; }
    public decimal? CurrentValue { get; set; }
    public decimal? Mileage { get; set; }
    public string? Status { get; set; }
    public int? CurrentBranchId { get; set; }
    public int? CurrentDriverId { get; set; }

    // ── Identification ──
    public string? Vin { get; set; }
    public string? EngineNumber { get; set; }
    public string? ChassisNumber { get; set; }
    public string? Color { get; set; }
    public int? SeatCount { get; set; }
    public string? FuelType { get; set; }

    // ── Registration ──
    public string? RegistrationNumber { get; set; }
    public string? RegistrationAuthority { get; set; }
    public string? RegistrationIssueDate { get; set; }
    public string? RegistrationExpiryDate { get; set; }
    public decimal? RegistrationCost { get; set; }
    public string? RegistrationNotes { get; set; }

    // ── Insurance / Warranty ──
    public string? InsurancePolicyNumber { get; set; }
    public string? InsuranceProvider { get; set; }
    public string? InsuranceStartDate { get; set; }
    public string? InsuranceExpiryDate { get; set; }
    public decimal? InsuranceCost { get; set; }
    public string? InsuranceCoverageDetails { get; set; }
    public string? WarrantyExpiryDate { get; set; }

    // ── Notes ──
    public string? Notes { get; set; }
}
