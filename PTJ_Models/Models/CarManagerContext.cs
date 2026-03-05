using System;
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

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Vehicle> Vehicles { get; set; }

    public virtual DbSet<VehicleAccessory> VehicleAccessories { get; set; }

    public virtual DbSet<VehicleDriverHistory> VehicleDriverHistories { get; set; }

    public virtual DbSet<VehicleModel> VehicleModels { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        => optionsBuilder.UseSqlServer("server =localhost; database = CarManager; uid=sa; pwd=123456;Trusted_Connection=True;Encrypt=False");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Accessory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__accessor__3213E83F22F58AD3");

            entity.ToTable("accessory");

            entity.HasIndex(e => e.Code, "UQ_accessory_code").IsUnique();

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
            entity.Property(e => e.Name)
                .HasMaxLength(200)
                .HasColumnName("name");
            entity.Property(e => e.QuantityInStock).HasColumnName("quantity_in_stock");
            entity.Property(e => e.UnitPrice)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("unit_price");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
        });

        modelBuilder.Entity<AssetChangeLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__asset_ch__3213E83FFC416E60");

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
                .HasConstraintName("FK_asset_change_user");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.AssetChangeLogs)
                .HasForeignKey(d => d.VehicleId)
                .HasConstraintName("FK_asset_change_vehicle");
        });

        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__branch__3213E83FF4B5BA82");

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
            entity.HasKey(e => e.Id).HasName("PK__bulk_pur__3213E83F1FE84336");

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
                .HasConstraintName("FK_bulk_branch");

            entity.HasOne(d => d.PurchaseProposal).WithMany(p => p.BulkPurchaseDetails)
                .HasForeignKey(d => d.PurchaseProposalId)
                .HasConstraintName("FK_bulk_purchase");
        });

        modelBuilder.Entity<CheckRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__check_re__3213E83F0F075E3F");

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
                .HasConstraintName("FK_check_operator");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.CheckRecords)
                .HasForeignKey(d => d.VehicleId)
                .HasConstraintName("FK_check_vehicle");
        });

        modelBuilder.Entity<DepreciationLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__deprecia__3213E83FC3ED3773");

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
                .HasConstraintName("FK_depr_user");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.DepreciationLogs)
                .HasForeignKey(d => d.VehicleId)
                .HasConstraintName("FK_depr_vehicle");
        });

        modelBuilder.Entity<DisposalProposal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__disposal__3213E83F4F01C1AB");

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
                .HasConstraintName("FK_disposal_manager");

            entity.HasOne(d => d.Proposer).WithMany(p => p.DisposalProposalProposers)
                .HasForeignKey(d => d.ProposerId)
                .HasConstraintName("FK_disposal_proposer");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.DisposalProposals)
                .HasForeignKey(d => d.VehicleId)
                .HasConstraintName("FK_disposal_vehicle");
        });

        modelBuilder.Entity<Driver>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__driver__3213E83F6282840C");

            entity.ToTable("driver");

            entity.HasIndex(e => e.LicenseNumber, "UQ_driver_license").IsUnique();

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
                .HasConstraintName("FK_driver_branch");
        });

        modelBuilder.Entity<EmailVerificationToken>(entity =>
        {
            entity.HasKey(e => e.EvtokenId).HasName("PK__EmailVer__A30302B82386EBF3");

            entity.HasIndex(e => e.Token, "IX_EmailVerificationTokens_Token").IsUnique();

            entity.HasIndex(e => e.UserId, "IX_EmailVerificationTokens_User_Active").HasFilter("([UsedAt] IS NULL)");

            entity.Property(e => e.EvtokenId).HasColumnName("EVTokenID");
            entity.Property(e => e.CreatedAt).HasColumnType("datetime");
            entity.Property(e => e.ExpiresAt).HasColumnType("datetime");
            entity.Property(e => e.Token).HasMaxLength(255);
            entity.Property(e => e.UsedAt).HasColumnType("datetime");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.EmailVerificationTokens)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK_EVT_user");
        });

        modelBuilder.Entity<InsuranceRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__insuranc__3213E83F806D7EFC");

            entity.ToTable("insurance_record");

            entity.HasIndex(e => e.PolicyNumber, "UQ_insurance_policy").IsUnique();

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
                .HasConstraintName("FK_insurance_vehicle");
        });

        modelBuilder.Entity<MaintenanceRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__maintena__3213E83F34DBC0DF");

            entity.ToTable("maintenance_request");

            entity.HasIndex(e => e.AccountantId, "IX_maintenance_accountant");

            entity.HasIndex(e => e.Status, "IX_maintenance_status");

            entity.HasIndex(e => e.VehicleId, "IX_maintenance_vehicle");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccountantId).HasColumnName("accountant_id");
            entity.Property(e => e.ActualCost)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("actual_cost");
            entity.Property(e => e.ApprovalNote)
                .HasMaxLength(500)
                .HasColumnName("approval_note");
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
            entity.Property(e => e.RejectionReason)
                .HasMaxLength(500)
                .HasColumnName("rejection_reason");
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
                .HasConstraintName("FK_maint_accountant");

            entity.HasOne(d => d.Operator).WithMany(p => p.MaintenanceRequestOperators)
                .HasForeignKey(d => d.OperatorId)
                .HasConstraintName("FK_maint_operator");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.MaintenanceRequests)
                .HasForeignKey(d => d.VehicleId)
                .HasConstraintName("FK_maint_vehicle");
        });

        modelBuilder.Entity<OverBudgetRepairProposal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__over_bud__3213E83F2917AFC1");

            entity.ToTable("over_budget_repair_proposal");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
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
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Maintenance).WithMany(p => p.OverBudgetRepairProposals)
                .HasForeignKey(d => d.MaintenanceId)
                .HasConstraintName("FK_overbudget_maint");

            entity.HasOne(d => d.Manager).WithMany(p => p.OverBudgetRepairProposals)
                .HasForeignKey(d => d.ManagerId)
                .HasConstraintName("FK_overbudget_manager");
        });

        modelBuilder.Entity<PurchaseProposal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__purchase__3213E83F7DEDAD28");

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
                .HasMaxLength(20)
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.ChiefAccountant).WithMany(p => p.PurchaseProposalChiefAccountants)
                .HasForeignKey(d => d.ChiefAccountantId)
                .HasConstraintName("FK_purchase_chief_accountant");

            entity.HasOne(d => d.Manager).WithMany(p => p.PurchaseProposalManagers)
                .HasForeignKey(d => d.ManagerId)
                .HasConstraintName("FK_purchase_manager");

            entity.HasOne(d => d.Proposer).WithMany(p => p.PurchaseProposalProposers)
                .HasForeignKey(d => d.ProposerId)
                .HasConstraintName("FK_purchase_proposer");
        });

        modelBuilder.Entity<RegistrationRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__registra__3213E83FBF7F1E4D");

            entity.ToTable("registration_record");

            entity.HasIndex(e => e.RegistrationNumber, "UQ_registration_number").IsUnique();

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
                .HasConstraintName("FK_registration_vehicle");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__role__3213E83FBE4F3A4C");

            entity.ToTable("role");

            entity.HasIndex(e => e.Name, "UQ_role_name").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<TransferPlan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__transfer__3213E83F3471F574");

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
                .HasConstraintName("FK_transfer_from");

            entity.HasOne(d => d.Manager).WithMany(p => p.TransferPlans)
                .HasForeignKey(d => d.ManagerId)
                .HasConstraintName("FK_transfer_manager");

            entity.HasOne(d => d.ToBranch).WithMany(p => p.TransferPlanToBranches)
                .HasForeignKey(d => d.ToBranchId)
                .HasConstraintName("FK_transfer_to");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.TransferPlans)
                .HasForeignKey(d => d.VehicleId)
                .HasConstraintName("FK_transfer_vehicle");
        });

        modelBuilder.Entity<TripLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__trip_log__3213E83F56C3B084");

            entity.ToTable("trip_log", tb => tb.HasTrigger("TRG_UpdateVehicleMileage"));

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
            entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");

            entity.HasOne(d => d.Driver).WithMany(p => p.TripLogs)
                .HasForeignKey(d => d.DriverId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_trip_driver");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.TripLogs)
                .HasForeignKey(d => d.VehicleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_trip_vehicle");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__user__3213E83F12645AB3");

            entity.ToTable("user");

            entity.HasIndex(e => e.Email, "UQ_user_email").IsUnique();

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
                .HasConstraintName("FK_user_branch");

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserRole",
                    r => r.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_user_role_role"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK_user_role_user"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId");
                        j.ToTable("user_role");
                        j.IndexerProperty<int>("UserId").HasColumnName("user_id");
                        j.IndexerProperty<int>("RoleId").HasColumnName("role_id");
                    });
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__vehicle__3213E83FD452066F");

            entity.ToTable("vehicle");

            entity.HasIndex(e => e.LicensePlate, "UQ_vehicle_license_plate").IsUnique();

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
                .HasConstraintName("FK_vehicle_branch");

            entity.HasOne(d => d.CurrentDriver).WithMany(p => p.Vehicles)
                .HasForeignKey(d => d.CurrentDriverId)
                .HasConstraintName("FK_vehicle_driver");

            entity.HasOne(d => d.Model).WithMany(p => p.Vehicles)
                .HasForeignKey(d => d.ModelId)
                .HasConstraintName("FK_vehicle_model");
        });

        modelBuilder.Entity<VehicleAccessory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__vehicle___3213E83F4E8DABE9");

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
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.RemoveDate).HasColumnName("remove_date");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");

            entity.HasOne(d => d.Accessory).WithMany(p => p.VehicleAccessories)
                .HasForeignKey(d => d.AccessoryId)
                .HasConstraintName("FK_vehicle_accessory_accessory");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.VehicleAccessories)
                .HasForeignKey(d => d.VehicleId)
                .HasConstraintName("FK_vehicle_accessory_vehicle");
        });

        modelBuilder.Entity<VehicleDriverHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__vehicle___3213E83FE5597254");

            entity.ToTable("vehicle_driver_history");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AssignDate).HasColumnName("assign_date");
            entity.Property(e => e.DriverId).HasColumnName("driver_id");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.UnassignDate).HasColumnName("unassign_date");
            entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");

            entity.HasOne(d => d.Driver).WithMany(p => p.VehicleDriverHistories)
                .HasForeignKey(d => d.DriverId)
                .HasConstraintName("FK_vdh_driver");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.VehicleDriverHistories)
                .HasForeignKey(d => d.VehicleId)
                .HasConstraintName("FK_vdh_vehicle");
        });

        modelBuilder.Entity<VehicleModel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__vehicle___3213E83F163A6036");

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

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
