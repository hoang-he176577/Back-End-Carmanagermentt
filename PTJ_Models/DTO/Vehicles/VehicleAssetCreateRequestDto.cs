using System;
using System.ComponentModel.DataAnnotations;

namespace Models.DTO.Vehicles;

public sealed class VehicleAssetCreateRequestDto
{
    // ===== THÔNG TIN XE CHÍNH =====

    [Required]
    [StringLength(20)]
    public string? LicensePlate { get; set; }

    [Required]
    public int? ModelId { get; set; }

    public int? YearManufacture { get; set; }

    public DateOnly? PurchaseDate { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? OriginalCost { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? CurrentValue { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? Mileage { get; set; }

    public string? Status { get; set; }

    public int? CurrentBranchId { get; set; }

    public int? CurrentDriverId { get; set; }

    // ===== ĐỊNH DANH XE =====

    [Required]
    [StringLength(50)]
    public string? Vin { get; set; }

    [Required]
    [StringLength(50)]
    public string? EngineNumber { get; set; }

    [Required]
    [StringLength(50)]
    public string? ChassisNumber { get; set; }

    [StringLength(50)]
    public string? Color { get; set; }

    public int? SeatCount { get; set; }

    [StringLength(30)]
    public string? FuelType { get; set; }

    // ===== ĐĂNG KÝ XE =====

    [StringLength(100)]
    public string? RegistrationNumber { get; set; }

    [StringLength(200)]
    public string? RegistrationAuthority { get; set; }

    public DateOnly? RegistrationIssueDate { get; set; }

    public DateOnly? RegistrationExpiryDate { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? RegistrationCost { get; set; }

    public string? RegistrationNotes { get; set; }

    // ===== BẢO HIỂM / BẢO HÀNH =====

    [StringLength(100)]
    public string? InsurancePolicyNumber { get; set; }

    [StringLength(200)]
    public string? InsuranceProvider { get; set; }

    public DateOnly? InsuranceStartDate { get; set; }

    public DateOnly? InsuranceExpiryDate { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? InsuranceCost { get; set; }

    public string? InsuranceCoverageDetails { get; set; }

    public DateOnly? WarrantyExpiryDate { get; set; }

    public string? Notes { get; set; }
}

