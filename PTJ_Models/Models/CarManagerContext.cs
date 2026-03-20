﻿using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Models.Models;

public partial class CarManagerContext : DbContext
{
    public CarManagerContext()
    {
    }

    public CarManagerContext(DbContextOptions<CarManagerContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Accessory> Accessories { get; set; }

    public virtual DbSet<AccessoryTransaction> AccessoryTransactions { get; set; }

    public virtual DbSet<AssetChangeLog> AssetChangeLogs { get; set; }

    public virtual DbSet<Branch> Branches { get; set; }

    public virtual DbSet<BulkPurchaseDetail> BulkPurchaseDetails { get; set; }

    public virtual DbSet<CheckRecord> CheckRecords { get; set; }

    public virtual DbSet<DepreciationLog> DepreciationLogs { get; set; }

    public virtual DbSet<DisposalProposal> DisposalProposals { get; set; }

    public virtual DbSet<Driver> Drivers { get; set; }

    public virtual DbSet<EmailVerificationToken> EmailVerificationTokens { get; set; }

    public virtual DbSet<InsuranceRecord> InsuranceRecords { get; set; }

    public virtual DbSet<MaintenanceRequest> MaintenanceRequests { get; set; }

    public virtual DbSet<OverBudgetRepairProposal> OverBudgetRepairProposals { get; set; }

    public virtual DbSet<PurchaseProposal> PurchaseProposals { get; set; }

    public virtual DbSet<RegistrationRecord> RegistrationRecords { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<TransferPlan> TransferPlans { get; set; }


    public virtual DbSet<TripLog> TripLogs { get; set; }

    public virtual DbSet<VehicleReceptionRecord> VehicleReceptionRecords { get; set; }


    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Vehicle> Vehicles { get; set; }

    public virtual DbSet<VehicleAccessory> VehicleAccessories { get; set; }

    public virtual DbSet<VehicleDriverHistory> VehicleDriverHistories { get; set; }

    public virtual DbSet<VehicleModel> VehicleModels { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Accessory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__accessor__3213E83F34B6220B");

            entity.ToTable("accessory");

            entity.HasIndex(e => e.Code, "UQ__accessor__357D4CF9738A1EEF").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Code)
                .HasMaxLength(50)
                .HasColumnName("code");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.IsActive)
                .HasDefaultValue(true)
                .HasColumnName("is_active");
            entity.Property(e => e.MinimumStock).HasColumnName("minimum_stock");
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.QuantityInStock).HasColumnName("quantity_in_stock");
            entity.Property(e => e.Type)
                .HasMaxLength(20)
                .HasDefaultValue("Reusable")
                .HasColumnName("type");
            entity.Property(e => e.UnitPrice)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("unit_price");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<AccessoryTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__accessor__3213E83FE5073449");

            entity.ToTable("accessory_transaction");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccessoryId).HasColumnName("accessory_id");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.PerformedBy).HasColumnName("performed_by");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.TransactionDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("transaction_date");
            entity.Property(e => e.TransactionType)
                .HasMaxLength(20)
                .HasColumnName("transaction_type");
            entity.Property(e => e.UnitPrice)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("unit_price");
            entity.Property(e => e.VehicleAccessoryId).HasColumnName("vehicle_accessory_id");
            entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");

            entity.HasOne(d => d.Accessory).WithMany(p => p.AccessoryTransactions)
                .HasForeignKey(d => d.AccessoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_accessory_transaction_accessory");

            entity.HasOne(d => d.PerformedByNavigation).WithMany(p => p.AccessoryTransactions)
                .HasForeignKey(d => d.PerformedBy)
                .HasConstraintName("FK_accessory_transaction_user");

            entity.HasOne(d => d.VehicleAccessory).WithMany(p => p.AccessoryTransactions)
                .HasForeignKey(d => d.VehicleAccessoryId)
                .HasConstraintName("FK_accessory_transaction_vehicle_accessory");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.AccessoryTransactions)
                .HasForeignKey(d => d.VehicleId)
                .HasConstraintName("FK_accessory_transaction_vehicle");
        });

        modelBuilder.Entity<AssetChangeLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__asset_ch__3213E83F6F7DBEBD");

            entity.ToTable("asset_change_log");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountantId).HasColumnName("accountant_id");
            entity.Property(e => e.AmountChange)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("amount_change");
            entity.Property(e => e.ChangeDate).HasColumnName("change_date");
            entity.Property(e => e.ChangeType)
                .HasMaxLength(20)
                .HasColumnName("change_type");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");

            entity.HasOne(d => d.Accountant).WithMany(p => p.AssetChangeLogs)
                .HasForeignKey(d => d.AccountantId)
                .HasConstraintName("FK__asset_cha__accou__2CF2ADDF");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.AssetChangeLogs)
                .HasForeignKey(d => d.VehicleId)
                .HasConstraintName("FK__asset_cha__vehic__2DE6D218");
        });

        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__branch__3213E83F3AD6F255");

            entity.ToTable("branch");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Address).HasColumnName("address");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<BulkPurchaseDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__bulk_pur__3213E83F3DDAE64D");

            entity.ToTable("bulk_purchase_detail");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity.Property(e => e.BranchNotes).HasColumnName("branch_notes");
            entity.Property(e => e.ProposedQuantity).HasColumnName("proposed_quantity");
            entity.Property(e => e.PurchaseProposalId).HasColumnName("purchase_proposal_id");
            entity.Property(e => e.ReceivedDate)
                .HasColumnType("datetime")
                .HasColumnName("received_date");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending")
                .HasColumnName("status");
            entity.Property(e => e.UnitPrice)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("unit_price");

            entity.HasOne(d => d.Branch).WithMany(p => p.BulkPurchaseDetails)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("FK__bulk_purc__branc__2EDAF651");

            entity.HasOne(d => d.PurchaseProposal).WithMany(p => p.BulkPurchaseDetails)
                .HasForeignKey(d => d.PurchaseProposalId)
                .HasConstraintName("FK__bulk_purc__purch__2FCF1A8A");
        });

        modelBuilder.Entity<CheckRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__check_re__3213E83F0107391E");

            entity.ToTable("check_record");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.MileageAtRecord)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("mileage_at_record");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.OperatorId).HasColumnName("operator_id");
            entity.Property(e => e.RecordDate)
                .HasColumnType("datetime")
                .HasColumnName("record_date");
            entity.Property(e => e.Type)
                .HasMaxLength(20)
                .HasColumnName("type");
            entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");

            entity.HasOne(d => d.Operator).WithMany(p => p.CheckRecords)
                .HasForeignKey(d => d.OperatorId)
                .HasConstraintName("FK__check_rec__opera__30C33EC3");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.CheckRecords)
                .HasForeignKey(d => d.VehicleId)
                .HasConstraintName("FK__check_rec__vehic__31B762FC");
        });

        modelBuilder.Entity<DepreciationLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__deprecia__3213E83FEF45F167");

            entity.ToTable("depreciation_log");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountantId).HasColumnName("accountant_id");
            entity.Property(e => e.CalculationDate).HasColumnName("calculation_date");
            entity.Property(e => e.DepreciationAmount)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("depreciation_amount");
            entity.Property(e => e.NewValue)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("new_value");
            entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");

            entity.HasOne(d => d.Accountant).WithMany(p => p.DepreciationLogs)
                .HasForeignKey(d => d.AccountantId)
                .HasConstraintName("FK__depreciat__accou__32AB8735");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.DepreciationLogs)
                .HasForeignKey(d => d.VehicleId)
                .HasConstraintName("FK__depreciat__vehic__339FAB6E");
        });

        modelBuilder.Entity<DisposalProposal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__disposal__3213E83F3AD7215B");

            entity.ToTable("disposal_proposal");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ApprovedDate).HasColumnName("approved_date");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedDate).HasColumnName("created_date");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.ManagerId).HasColumnName("manager_id");
            entity.Property(e => e.ProposedPrice)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("proposed_price");
            entity.Property(e => e.ProposerId).HasColumnName("proposer_id");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");

            entity.HasOne(d => d.Manager).WithMany(p => p.DisposalProposalManagers)
                .HasForeignKey(d => d.ManagerId)
                .HasConstraintName("FK__disposal___manag__3493CFA7");

            entity.HasOne(d => d.Proposer).WithMany(p => p.DisposalProposalProposers)
                .HasForeignKey(d => d.ProposerId)
                .HasConstraintName("FK__disposal___propo__3587F3E0");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.DisposalProposals)
                .HasForeignKey(d => d.VehicleId)
                .HasConstraintName("FK__disposal___vehic__367C1819");
        });

        modelBuilder.Entity<Driver>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__driver__3213E83F7E717913");

            entity.ToTable("driver");

            entity.HasIndex(e => e.LicenseNumber, "UQ__driver__D482A003CC18CE3E").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.HireDate).HasColumnName("hire_date");
            entity.Property(e => e.LicenseNumber)
                .HasMaxLength(50)
                .HasColumnName("license_number");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Branch).WithMany(p => p.Drivers)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("FK__driver__branch_i__37703C52");
        });

        modelBuilder.Entity<EmailVerificationToken>(entity =>
        {
            entity.HasKey(e => e.EvtokenId);

            entity.HasIndex(e => e.Token, "IX_EmailVerificationTokens_Token").IsUnique();

            entity.HasIndex(e => e.UserId, "IX_EmailVerificationTokens_User_Active").HasFilter("([UsedAt] IS NULL)");

            entity.Property(e => e.EvtokenId).HasColumnName("EVTokenID");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime");
            entity.Property(e => e.ExpiresAt).HasColumnType("datetime");
            entity.Property(e => e.Token).HasMaxLength(255);
            entity.Property(e => e.UsedAt).HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.EmailVerificationTokens)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_EmailVerificationTokens_User");
        });

        modelBuilder.Entity<InsuranceRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__insuranc__3213E83F79A3D2FD");

            entity.ToTable("insurance_record");

            entity.HasIndex(e => e.PolicyNumber, "UQ__insuranc__96916872638A2F52").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Cost)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("cost");
            entity.Property(e => e.CoverageDetails).HasColumnName("coverage_details");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.ExpiryDate).HasColumnName("expiry_date");
            entity.Property(e => e.PolicyNumber)
                .HasMaxLength(50)
                .HasColumnName("policy_number");
            entity.Property(e => e.Provider)
                .HasMaxLength(100)
                .HasColumnName("provider");
            entity.Property(e => e.StartDate).HasColumnName("start_date");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.InsuranceRecords)
                .HasForeignKey(d => d.VehicleId)
                .HasConstraintName("FK__insurance__vehic__395884C4");
        });

        modelBuilder.Entity<MaintenanceRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__maintena__3213E83F275AC305");

            entity.ToTable("maintenance_request");

            entity.HasIndex(e => e.AccountantId, "IX_maintenance_accountant");

            entity.HasIndex(e => e.Status, "IX_maintenance_status");

            entity.HasIndex(e => e.VehicleId, "IX_maintenance_vehicle");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountantId).HasColumnName("accountant_id");
            entity.Property(e => e.ActualCost)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("actual_cost");
            entity.Property(e => e.ApprovedDate).HasColumnName("approved_date");
            entity.Property(e => e.CompletionDate).HasColumnName("completion_date");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.EstimatedCost)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("estimated_cost");
            entity.Property(e => e.MaintenanceType)
                .HasMaxLength(20)
                .HasDefaultValue("Breakdown")
                .HasColumnName("maintenance_type");
            entity.Property(e => e.OperatorId).HasColumnName("operator_id");
            entity.Property(e => e.RequestDate).HasColumnName("request_date");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");

            entity.HasOne(d => d.Accountant).WithMany(p => p.MaintenanceRequestAccountants)
                .HasForeignKey(d => d.AccountantId)
                .HasConstraintName("FK_maintenance_request_accountant");

            entity.HasOne(d => d.Operator).WithMany(p => p.MaintenanceRequestOperators)
                .HasForeignKey(d => d.OperatorId)
                .HasConstraintName("FK__maintenan__opera__3A4CA8FD");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.MaintenanceRequests)
                .HasForeignKey(d => d.VehicleId)
                .HasConstraintName("FK__maintenan__vehic__3B40CD36");
        });

        modelBuilder.Entity<OverBudgetRepairProposal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__over_bud__3213E83F4A7D9467");

            entity.ToTable("over_budget_repair_proposal");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.MaintenanceId).HasColumnName("maintenance_id");
            entity.Property(e => e.ManagerId).HasColumnName("manager_id");
            entity.Property(e => e.OverBudgetAmount)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("over_budget_amount");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Maintenance).WithMany(p => p.OverBudgetRepairProposals)
                .HasForeignKey(d => d.MaintenanceId)
                .HasConstraintName("FK__over_budg__maint__3D2915A8");

            entity.HasOne(d => d.Manager).WithMany(p => p.OverBudgetRepairProposals)
                .HasForeignKey(d => d.ManagerId)
                .HasConstraintName("FK__over_budg__manag__3E1D39E1");
        });

        modelBuilder.Entity<PurchaseProposal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__purchase__3213E83FAE698483");

            entity.ToTable("purchase_proposal");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ApprovedDate).HasColumnName("approved_date");
            entity.Property(e => e.ChiefAccountantId).HasColumnName("chief_accountant_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedDate).HasColumnName("created_date");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ManagerId).HasColumnName("manager_id");
            entity.Property(e => e.ProposedCost)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("proposed_cost");
            entity.Property(e => e.ProposerId).HasColumnName("proposer_id");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.ChiefAccountant).WithMany(p => p.PurchaseProposalChiefAccountants)
                .HasForeignKey(d => d.ChiefAccountantId)
                .HasConstraintName("FK__purchase___chief__3F115E1A");

            entity.HasOne(d => d.Manager).WithMany(p => p.PurchaseProposalManagers)
                .HasForeignKey(d => d.ManagerId)
                .HasConstraintName("FK__purchase___manag__40058253");

            entity.HasOne(d => d.Proposer).WithMany(p => p.PurchaseProposalProposers)
                .HasForeignKey(d => d.ProposerId)
                .HasConstraintName("FK__purchase___propo__40F9A68C");
        });

        modelBuilder.Entity<RegistrationRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__registra__3213E83F7F279DE8");

            entity.ToTable("registration_record");

            entity.HasIndex(e => e.RegistrationNumber, "UQ__registra__125DB2A3BA1B551E").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Authority)
                .HasMaxLength(100)
                .HasColumnName("authority");
            entity.Property(e => e.Cost)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("cost");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.ExpiryDate).HasColumnName("expiry_date");
            entity.Property(e => e.IssueDate).HasColumnName("issue_date");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.RegistrationNumber)
                .HasMaxLength(50)
                .HasColumnName("registration_number");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.RegistrationRecords)
                .HasForeignKey(d => d.VehicleId)
                .HasConstraintName("FK__registrat__vehic__41EDCAC5");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__role__3213E83F80178E76");

            entity.ToTable("role");

            entity.HasIndex(e => e.Name, "UQ__role__72E12F1BF0A2EAA6").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<TransferPlan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__transfer__3213E83F8E568365");

            entity.ToTable("transfer_plan");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.ExecutedDate).HasColumnName("executed_date");
            entity.Property(e => e.FromBranchId).HasColumnName("from_branch_id");
            entity.Property(e => e.ManagerId).HasColumnName("manager_id");
            entity.Property(e => e.PlanDate).HasColumnName("plan_date");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
            entity.Property(e => e.ToBranchId).HasColumnName("to_branch_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");

            entity.HasOne(d => d.FromBranch).WithMany(p => p.TransferPlanFromBranches)
                .HasForeignKey(d => d.FromBranchId)
                .HasConstraintName("FK__transfer___from___42E1EEFE");

            entity.HasOne(d => d.Manager).WithMany(p => p.TransferPlans)
                .HasForeignKey(d => d.ManagerId)
                .HasConstraintName("FK__transfer___manag__43D61337");

            entity.HasOne(d => d.ToBranch).WithMany(p => p.TransferPlanToBranches)
                .HasForeignKey(d => d.ToBranchId)
                .HasConstraintName("FK__transfer___to_br__44CA3770");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.TransferPlans)
                .HasForeignKey(d => d.VehicleId)
                .HasConstraintName("FK__transfer___vehic__45BE5BA9");
        });

        modelBuilder.Entity<TripLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__trip_log__3213E83F3D094675");

            entity.ToTable("trip_log");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Destination)
                .HasMaxLength(255)
                .HasColumnName("destination");
            entity.Property(e => e.DriverId).HasColumnName("driver_id");
            entity.Property(e => e.EndMileage)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("end_mileage");
            entity.Property(e => e.EndTime)
                .HasColumnType("datetime")
                .HasColumnName("end_time");
            entity.Property(e => e.EndedBy).HasColumnName("ended_by");
            entity.Property(e => e.Origin)
                .HasMaxLength(255)
                .HasColumnName("origin");
            entity.Property(e => e.Purpose).HasColumnName("purpose");
            entity.Property(e => e.StartMileage)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("start_mileage");
            entity.Property(e => e.StartTime)
                .HasColumnType("datetime")
                .HasColumnName("start_time");
            entity.Property(e => e.StartedBy)
                .HasColumnName("started_by")
                .HasDefaultValue(0);
            entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");

            entity.HasOne(d => d.Driver).WithMany(p => p.TripLogs)
                .HasForeignKey(d => d.DriverId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_triplog_driver");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.TripLogs)
                .HasForeignKey(d => d.VehicleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_triplog_vehicle");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__user__3213E83F2B3DE7B9");

            entity.ToTable("user");

            entity.HasIndex(e => e.Email, "UQ__user__AB6E6164767588DD").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.Email)
                .HasMaxLength(100)
                .HasColumnName("email");
            entity.Property(e => e.EmailVerified)
                .HasDefaultValue(false)
                .HasColumnName("email_verified");
            entity.Property(e => e.LastLogin)
                .HasColumnType("datetime")
                .HasColumnName("last_login");
            entity.Property(e => e.Name)
                .HasMaxLength(100)
                .HasColumnName("name");
            entity.Property(e => e.PasswordHash)
                .HasMaxLength(255)
                .HasColumnName("password_hash");
            entity.Property(e => e.Phone)
                .HasMaxLength(20)
                .HasColumnName("phone");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Branch).WithMany(p => p.Users)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("FK__user__branch_id__46B27FE2");

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserRole",
                    r => r.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__user_role__role___47A6A41B"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__user_role__user___489AC854"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId").HasName("PK__user_rol__6EDEA153BB663647");
                        j.ToTable("user_role");
                        j.IndexerProperty<int>("UserId").HasColumnName("user_id");
                        j.IndexerProperty<int>("RoleId").HasColumnName("role_id");
                    });
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__vehicle__3213E83F43A346AE");

            entity.ToTable("vehicle");

            entity.HasIndex(e => e.LicensePlate, "UQ__vehicle__F72CD56EABBE1952").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CurrentBranchId).HasColumnName("current_branch_id");
            entity.Property(e => e.CurrentDriverId).HasColumnName("current_driver_id");
            entity.Property(e => e.CurrentValue)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("current_value");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.LicensePlate)
                .HasMaxLength(20)
                .HasColumnName("license_plate");
            entity.Property(e => e.Mileage)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("mileage");
            entity.Property(e => e.ModelId).HasColumnName("model_id");
            entity.Property(e => e.OriginalCost)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("original_cost");
            entity.Property(e => e.PurchaseDate).HasColumnName("purchase_date");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.YearManufacture).HasColumnName("year_manufacture");

            entity.HasOne(d => d.CurrentBranch).WithMany(p => p.Vehicles)
                .HasForeignKey(d => d.CurrentBranchId)
                .HasConstraintName("FK__vehicle__current__498EEC8D");

            entity.HasOne(d => d.CurrentDriver).WithMany(p => p.Vehicles)
                .HasForeignKey(d => d.CurrentDriverId)
                .HasConstraintName("FK__vehicle__current__4A8310C6");

            entity.HasOne(d => d.Model).WithMany(p => p.Vehicles)
                .HasForeignKey(d => d.ModelId)
                .HasConstraintName("FK__vehicle__model_i__4B7734FF");
        });

        modelBuilder.Entity<VehicleAccessory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__vehicle___3213E83F0E5491A5");

            entity.ToTable("vehicle_accessory");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccessoryId).HasColumnName("accessory_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.InstallDate).HasColumnName("install_date");
            entity.Property(e => e.InstalledBy).HasColumnName("installed_by");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.Quantity)
                .HasDefaultValue(1)
                .HasColumnName("quantity");
            entity.Property(e => e.RemoveDate).HasColumnName("remove_date");
            entity.Property(e => e.RemovedBy).HasColumnName("removed_by");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Installed")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");

            entity.HasOne(d => d.Accessory).WithMany(p => p.VehicleAccessories)
                .HasForeignKey(d => d.AccessoryId)
                .HasConstraintName("FK__vehicle_a__acces__4C6B5938");

            entity.HasOne(d => d.InstalledByNavigation).WithMany(p => p.VehicleAccessoryInstalledByNavigations)
                .HasForeignKey(d => d.InstalledBy)
                .HasConstraintName("FK_vehicle_accessory_installed_by");

            entity.HasOne(d => d.RemovedByNavigation).WithMany(p => p.VehicleAccessoryRemovedByNavigations)
                .HasForeignKey(d => d.RemovedBy)
                .HasConstraintName("FK_vehicle_accessory_removed_by");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.VehicleAccessories)
                .HasForeignKey(d => d.VehicleId)
                .HasConstraintName("FK__vehicle_a__vehic__4D5F7D71");
        });

        modelBuilder.Entity<VehicleDriverHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__vehicle___3213E83F2EBD6455");

            entity.ToTable("vehicle_driver_history");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AssignDate).HasColumnName("assign_date");
            entity.Property(e => e.DriverId).HasColumnName("driver_id");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.UnassignDate).HasColumnName("unassign_date");
            entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");

            entity.HasOne(d => d.Driver).WithMany(p => p.VehicleDriverHistories)
                .HasForeignKey(d => d.DriverId)
                .HasConstraintName("FK__vehicle_d__drive__4E53A1AA");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.VehicleDriverHistories)
                .HasForeignKey(d => d.VehicleId)
                .HasConstraintName("FK__vehicle_d__vehic__4F47C5E3");
        });

        modelBuilder.Entity<VehicleModel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__vehicle___3213E83F7537AE2C");

            entity.ToTable("vehicle_model");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DefaultPrice)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("default_price");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.EngineType)
                .HasMaxLength(50)
                .HasColumnName("engine_type");
            entity.Property(e => e.Manufacturer)
                .HasMaxLength(100)
                .HasColumnName("manufacturer");
            entity.Property(e => e.ModelName)
                .HasMaxLength(100)
                .HasColumnName("model_name");
            entity.Property(e => e.Seats).HasColumnName("seats");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.YearFrom).HasColumnName("year_from");
            entity.Property(e => e.YearTo).HasColumnName("year_to");
        });

        // ===== VEHICLE RECEPTION RECORD =====
        modelBuilder.Entity<VehicleReceptionRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__vehicle_reception__ID");

            entity.ToTable("vehicle_reception_record");

            // Cấu hình Indexes (Chỉ mục) từ SQL Script
            entity.HasIndex(e => e.PurchaseProposalId, "IX_VehicleReception_PurchaseProposalId");
            entity.HasIndex(e => e.BranchId, "IX_VehicleReception_BranchId");
            entity.HasIndex(e => e.Status, "IX_VehicleReception_Status");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.PurchaseProposalId).HasColumnName("purchase_proposal_id");
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity.Property(e => e.OperatorId).HasColumnName("operator_id");
            entity.Property(e => e.RequestedDate).HasColumnName("requested_date");
            entity.Property(e => e.ReceivedDate).HasColumnName("received_date");
            entity.Property(e => e.LicensePlate)
                .HasMaxLength(50)
                .HasColumnName("license_plate");
            entity.Property(e => e.ChassisNumber)
                .HasMaxLength(100)
                .HasColumnName("chassis_number");
            entity.Property(e => e.EngineNumber)
                .HasMaxLength(100)
                .HasColumnName("engine_number");
            entity.Property(e => e.ReceiptImageUrl)
                .HasColumnName("receipt_image_url");
            entity.Property(e => e.Notes)
                .HasMaxLength(1000)
                .HasColumnName("notes");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasColumnName("status");
            entity.Property(e => e.Reason)
                .HasColumnName("reason");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");

            entity.HasOne(d => d.PurchaseProposal)
                .WithMany()
                .HasForeignKey(d => d.PurchaseProposalId)
                .HasConstraintName("FK__vehicle_reception__proposal");

            entity.HasOne(d => d.Branch)
                .WithMany()
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("FK__vehicle_reception__branch");

            entity.HasOne(d => d.Operator)
                .WithMany()
                .HasForeignKey(d => d.OperatorId)
                .HasConstraintName("FK__vehicle_reception__operator");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
