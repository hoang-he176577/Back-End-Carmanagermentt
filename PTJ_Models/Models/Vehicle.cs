using System;
using System.Collections.Generic;

namespace Models.Models;

public partial class Vehicle
{
    public int Id { get; set; }

    public string? LicensePlate { get; set; }

    public int? ModelId { get; set; }

    public int? YearManufacture { get; set; }

    public DateOnly? PurchaseDate { get; set; }

    public decimal? OriginalCost { get; set; }

    public decimal? CurrentValue { get; set; }

    public decimal? Mileage { get; set; }

    public string? Status { get; set; }

    public int? CurrentBranchId { get; set; }

    public int? CurrentDriverId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<AccessoryTransaction> AccessoryTransactions { get; set; } = new List<AccessoryTransaction>();

    public virtual ICollection<AssetChangeLog> AssetChangeLogs { get; set; } = new List<AssetChangeLog>();

    public virtual ICollection<CheckRecord> CheckRecords { get; set; } = new List<CheckRecord>();

    public virtual Branch? CurrentBranch { get; set; }

    public virtual Driver? CurrentDriver { get; set; }

    public virtual ICollection<DepreciationLog> DepreciationLogs { get; set; } = new List<DepreciationLog>();

    public virtual ICollection<DisposalProposal> DisposalProposals { get; set; } = new List<DisposalProposal>();

    public virtual ICollection<InsuranceRecord> InsuranceRecords { get; set; } = new List<InsuranceRecord>();

    public virtual ICollection<MaintenanceRequest> MaintenanceRequests { get; set; } = new List<MaintenanceRequest>();

    public virtual VehicleModel? Model { get; set; }

    public virtual ICollection<RegistrationRecord> RegistrationRecords { get; set; } = new List<RegistrationRecord>();

    public virtual ICollection<TransferPlan> TransferPlans { get; set; } = new List<TransferPlan>();

    public virtual ICollection<TripLog> TripLogs { get; set; } = new List<TripLog>();

    public virtual ICollection<VehicleAccessory> VehicleAccessories { get; set; } = new List<VehicleAccessory>();

    public virtual ICollection<VehicleDriverHistory> VehicleDriverHistories { get; set; } = new List<VehicleDriverHistory>();
}
