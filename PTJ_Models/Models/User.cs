using System;
using System.Collections.Generic;

namespace Models.Models;

public partial class User
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public int? BranchId { get; set; }

    public string? PasswordHash { get; set; }

    public bool? EmailVerified { get; set; }

    public DateTime? LastLogin { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<AccessoryGoodsReceipt> AccessoryGoodsReceipts { get; set; } = new List<AccessoryGoodsReceipt>();

    public virtual ICollection<AccessoryPurchaseRequest> AccessoryPurchaseRequestApprovedBies { get; set; } = new List<AccessoryPurchaseRequest>();

    public virtual ICollection<AccessoryPurchaseRequest> AccessoryPurchaseRequestRequesters { get; set; } = new List<AccessoryPurchaseRequest>();

    public virtual ICollection<AccessoryTransaction> AccessoryTransactions { get; set; } = new List<AccessoryTransaction>();

    public virtual ICollection<AssetChangeLog> AssetChangeLogs { get; set; } = new List<AssetChangeLog>();

    public virtual Branch? Branch { get; set; }

    public virtual ICollection<CheckRecord> CheckRecords { get; set; } = new List<CheckRecord>();

    public virtual ICollection<DepreciationLog> DepreciationLogs { get; set; } = new List<DepreciationLog>();

    public virtual ICollection<DisposalProposal> DisposalProposalManagers { get; set; } = new List<DisposalProposal>();

    public virtual ICollection<DisposalProposal> DisposalProposalProposers { get; set; } = new List<DisposalProposal>();

    public virtual ICollection<DriverTransferDetail> DriverTransferDetailConfirmedByUsers { get; set; } = new List<DriverTransferDetail>();

    public virtual ICollection<DriverTransferRequest> DriverTransferRequestCreatedByUsers { get; set; } = new List<DriverTransferRequest>();

    public virtual ICollection<EmailVerificationToken> EmailVerificationTokens { get; set; } = new List<EmailVerificationToken>();

    public virtual ICollection<MaintenanceRequest> MaintenanceRequestAccountants { get; set; } = new List<MaintenanceRequest>();

    public virtual ICollection<MaintenanceRequest> MaintenanceRequestOperators { get; set; } = new List<MaintenanceRequest>();

    public virtual ICollection<OverBudgetRepairProposal> OverBudgetRepairProposals { get; set; } = new List<OverBudgetRepairProposal>();

    public virtual ICollection<PurchaseProposal> PurchaseProposalChiefAccountants { get; set; } = new List<PurchaseProposal>();

    public virtual ICollection<PurchaseProposal> PurchaseProposalManagers { get; set; } = new List<PurchaseProposal>();

    public virtual ICollection<PurchaseProposal> PurchaseProposalProposers { get; set; } = new List<PurchaseProposal>();

    public virtual ICollection<TransferPlan> TransferPlanCheckinByUsers { get; set; } = new List<TransferPlan>();

    public virtual ICollection<TransferPlan> TransferPlanCheckoutByUsers { get; set; } = new List<TransferPlan>();

    public virtual ICollection<TransferPlan> TransferPlanManagers { get; set; } = new List<TransferPlan>();

    public virtual ICollection<VehicleAccessory> VehicleAccessoryInstalledByNavigations { get; set; } = new List<VehicleAccessory>();

    public virtual ICollection<VehicleAccessory> VehicleAccessoryRemovedByNavigations { get; set; } = new List<VehicleAccessory>();

    public virtual ICollection<VehicleReceptionRecord> VehicleReceptionRecords { get; set; } = new List<VehicleReceptionRecord>();

    public virtual ICollection<Role> Roles { get; set; } = new List<Role>();
}
