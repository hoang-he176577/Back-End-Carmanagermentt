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

    public virtual DbSet<InsuranceRecord> InsuranceRecords { get; set; }

    public virtual DbSet<MaintenanceRequest> MaintenanceRequests { get; set; }

    public virtual DbSet<OverBudgetRepairProposal> OverBudgetRepairProposals { get; set; }

    public virtual DbSet<PurchaseProposal> PurchaseProposals { get; set; }

    public virtual DbSet<RegistrationRecord> RegistrationRecords { get; set; }

    public virtual DbSet<Role> Roles { get; set; }

    public virtual DbSet<TransferPlan> TransferPlans { get; set; }

    public virtual DbSet<User> Users { get; set; }

    public virtual DbSet<Vehicle> Vehicles { get; set; }

    public virtual DbSet<VehicleAccessory> VehicleAccessories { get; set; }

    public virtual DbSet<VehicleDriverHistory> VehicleDriverHistories { get; set; }

    public virtual DbSet<VehicleModel> VehicleModels { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
#warning To protect potentially sensitive information in your connection string, you should move it out of source code. You can avoid scaffolding the connection string by using the Name= syntax to read it from configuration - see https://go.microsoft.com/fwlink/?linkid=2131148. For more guidance on storing connection strings, see https://go.microsoft.com/fwlink/?LinkId=723263.
        if (!optionsBuilder.IsConfigured)
        {
            optionsBuilder.UseSqlServer("server =(local); database = CarManager; uid=sa;pwd=123456;Trusted_Connection=True;Encrypt=False");
        }
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Accessory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__accessor__3213E83FB36A25E9");

            entity.ToTable("accessory");

            entity.HasIndex(e => e.Code, "UQ__accessor__357D4CF9071824EE").IsUnique();

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
            entity.HasKey(e => e.Id).HasName("PK__asset_ch__3213E83F9A0E9B11");

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
                .HasConstraintName("FK__asset_cha__accou__0A9D95DB");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.AssetChangeLogs)
                .HasForeignKey(d => d.VehicleId)
                .HasConstraintName("FK__asset_cha__vehic__09A971A2");
        });

        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__branch__3213E83FB8943FDA");

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
            entity.HasKey(e => e.Id).HasName("PK__bulk_pur__3213E83F88ABFAA2");

            entity.ToTable("bulk_purchase_detail");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity.Property(e => e.BranchNotes).HasColumnName("branch_notes");
            entity.Property(e => e.ProposedQuantity).HasColumnName("proposed_quantity");
            entity.Property(e => e.PurchaseProposalId).HasColumnName("purchase_proposal_id");

            entity.HasOne(d => d.Branch).WithMany(p => p.BulkPurchaseDetails)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("FK__bulk_purc__branc__656C112C");

            entity.HasOne(d => d.PurchaseProposal).WithMany(p => p.BulkPurchaseDetails)
                .HasForeignKey(d => d.PurchaseProposalId)
                .HasConstraintName("FK__bulk_purc__purch__6477ECF3");
        });

        modelBuilder.Entity<CheckRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__check_re__3213E83FBF7B80F0");

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
                .HasConstraintName("FK__check_rec__opera__02FC7413");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.CheckRecords)
                .HasForeignKey(d => d.VehicleId)
                .HasConstraintName("FK__check_rec__vehic__02084FDA");
        });

        modelBuilder.Entity<DepreciationLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__deprecia__3213E83FBFE456DF");

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
                .HasConstraintName("FK__depreciat__accou__06CD04F7");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.DepreciationLogs)
                .HasForeignKey(d => d.VehicleId)
                .HasConstraintName("FK__depreciat__vehic__05D8E0BE");
        });

        modelBuilder.Entity<DisposalProposal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__disposal__3213E83FAE13CC5F");

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
                .HasConstraintName("FK__disposal___manag__6C190EBB");

            entity.HasOne(d => d.Proposer).WithMany(p => p.DisposalProposalProposers)
                .HasForeignKey(d => d.ProposerId)
                .HasConstraintName("FK__disposal___propo__6B24EA82");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.DisposalProposals)
                .HasForeignKey(d => d.VehicleId)
                .HasConstraintName("FK__disposal___vehic__6A30C649");
        });

        modelBuilder.Entity<Driver>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__driver__3213E83FFDBD6CE8");

            entity.ToTable("driver");

            entity.HasIndex(e => e.LicenseNumber, "UQ__driver__D482A003CD535E79").IsUnique();

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
                .HasConstraintName("FK__driver__branch_i__4BAC3F29");
        });

        modelBuilder.Entity<InsuranceRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__insuranc__3213E83F649FEB2E");

            entity.ToTable("insurance_record");

            entity.HasIndex(e => e.PolicyNumber, "UQ__insuranc__96916872023F60D7").IsUnique();

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
                .HasConstraintName("FK__insurance__vehic__1AD3FDA4");
        });

        modelBuilder.Entity<MaintenanceRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__maintena__3213E83FFEE8F090");

            entity.ToTable("maintenance_request");

            entity.Property(e => e.Id).HasColumnName("id");
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

            entity.HasOne(d => d.Operator).WithMany(p => p.MaintenanceRequests)
                .HasForeignKey(d => d.OperatorId)
                .HasConstraintName("FK__maintenan__opera__71D1E811");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.MaintenanceRequests)
                .HasForeignKey(d => d.VehicleId)
                .HasConstraintName("FK__maintenan__vehic__70DDC3D8");
        });

        modelBuilder.Entity<OverBudgetRepairProposal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__over_bud__3213E83F54468F62");

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
                .HasConstraintName("FK__over_budg__maint__76969D2E");

            entity.HasOne(d => d.Manager).WithMany(p => p.OverBudgetRepairProposals)
                .HasForeignKey(d => d.ManagerId)
                .HasConstraintName("FK__over_budg__manag__778AC167");
        });

        modelBuilder.Entity<PurchaseProposal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__purchase__3213E83FDFB7E59D");

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
                .HasConstraintName("FK__purchase___chief__619B8048");

            entity.HasOne(d => d.Manager).WithMany(p => p.PurchaseProposalManagers)
                .HasForeignKey(d => d.ManagerId)
                .HasConstraintName("FK__purchase___manag__60A75C0F");

            entity.HasOne(d => d.Proposer).WithMany(p => p.PurchaseProposalProposers)
                .HasForeignKey(d => d.ProposerId)
                .HasConstraintName("FK__purchase___propo__5FB337D6");
        });

        modelBuilder.Entity<RegistrationRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__registra__3213E83FE6E95FEF");

            entity.ToTable("registration_record");

            entity.HasIndex(e => e.RegistrationNumber, "UQ__registra__125DB2A35D11E8BC").IsUnique();

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
                .HasConstraintName("FK__registrat__vehic__208CD6FA");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__role__3213E83F05B627F4");

            entity.ToTable("role");

            entity.HasIndex(e => e.Name, "UQ__role__72E12F1BA3C40210").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<TransferPlan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__transfer__3213E83FCC805707");

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
                .HasConstraintName("FK__transfer___from___7D439ABD");

            entity.HasOne(d => d.Manager).WithMany(p => p.TransferPlans)
                .HasForeignKey(d => d.ManagerId)
                .HasConstraintName("FK__transfer___manag__7F2BE32F");

            entity.HasOne(d => d.ToBranch).WithMany(p => p.TransferPlanToBranches)
                .HasForeignKey(d => d.ToBranchId)
                .HasConstraintName("FK__transfer___to_br__7E37BEF6");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.TransferPlans)
                .HasForeignKey(d => d.VehicleId)
                .HasConstraintName("FK__transfer___vehic__7C4F7684");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__user__3213E83F9F82830B");

            entity.ToTable("user");

            entity.HasIndex(e => e.Email, "UQ__user__AB6E6164078874B0").IsUnique();

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
                .HasConstraintName("FK__user__branch_id__4222D4EF");

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserRole",
                    r => r.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__user_role__role___45F365D3"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__user_role__user___44FF419A"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId").HasName("PK__user_rol__6EDEA153A59FE198");
                        j.ToTable("user_role");
                        j.IndexerProperty<int>("UserId").HasColumnName("user_id");
                        j.IndexerProperty<int>("RoleId").HasColumnName("role_id");
                    });
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__vehicle__3213E83FE10D0D00");

            entity.ToTable("vehicle");

            entity.HasIndex(e => e.LicensePlate, "UQ__vehicle__F72CD56EDBC929E2").IsUnique();

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
                .HasConstraintName("FK__vehicle__current__5629CD9C");

            entity.HasOne(d => d.CurrentDriver).WithMany(p => p.Vehicles)
                .HasForeignKey(d => d.CurrentDriverId)
                .HasConstraintName("FK__vehicle__current__571DF1D5");

            entity.HasOne(d => d.Model).WithMany(p => p.Vehicles)
                .HasForeignKey(d => d.ModelId)
                .HasConstraintName("FK__vehicle__model_i__5535A963");
        });

        modelBuilder.Entity<VehicleAccessory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__vehicle___3213E83FC07CA7E3");

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
                .HasConstraintName("FK__vehicle_a__acces__151B244E");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.VehicleAccessories)
                .HasForeignKey(d => d.VehicleId)
                .HasConstraintName("FK__vehicle_a__vehic__14270015");
        });

        modelBuilder.Entity<VehicleDriverHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__vehicle___3213E83FDF646473");

            entity.ToTable("vehicle_driver_history");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AssignDate).HasColumnName("assign_date");
            entity.Property(e => e.DriverId).HasColumnName("driver_id");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.UnassignDate).HasColumnName("unassign_date");
            entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");

            entity.HasOne(d => d.Driver).WithMany(p => p.VehicleDriverHistories)
                .HasForeignKey(d => d.DriverId)
                .HasConstraintName("FK__vehicle_d__drive__5AEE82B9");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.VehicleDriverHistories)
                .HasForeignKey(d => d.VehicleId)
                .HasConstraintName("FK__vehicle_d__vehic__59FA5E80");
        });

        modelBuilder.Entity<VehicleModel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__vehicle___3213E83FA93B96A1");

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
