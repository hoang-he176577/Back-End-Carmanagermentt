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

    public virtual DbSet<AccessoryGoodsReceipt> AccessoryGoodsReceipts { get; set; }

    public virtual DbSet<AccessoryGoodsReceiptDetail> AccessoryGoodsReceiptDetails { get; set; }

    public virtual DbSet<AccessoryPurchaseRequest> AccessoryPurchaseRequests { get; set; }

    public virtual DbSet<AccessoryPurchaseRequestDetail> AccessoryPurchaseRequestDetails { get; set; }

    public virtual DbSet<AccessoryTransaction> AccessoryTransactions { get; set; }

    public virtual DbSet<AssetChangeLog> AssetChangeLogs { get; set; }

    public virtual DbSet<Branch> Branches { get; set; }

    public virtual DbSet<BranchAccessoryStock> BranchAccessoryStocks { get; set; }

    public virtual DbSet<BulkPurchaseDetail> BulkPurchaseDetails { get; set; }

    public virtual DbSet<CheckRecord> CheckRecords { get; set; }

    public virtual DbSet<DepreciationLog> DepreciationLogs { get; set; }

    public virtual DbSet<DisposalProposal> DisposalProposals { get; set; }

    public virtual DbSet<Driver> Drivers { get; set; }

    public virtual DbSet<DriverTransferDetail> DriverTransferDetails { get; set; }

    public virtual DbSet<DriverTransferRequest> DriverTransferRequests { get; set; }

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

    public virtual DbSet<VehicleAccessoryRequirement> VehicleAccessoryRequirements { get; set; }

    public virtual DbSet<VehicleDriverHistory> VehicleDriverHistories { get; set; }

    public virtual DbSet<VehicleModel> VehicleModels { get; set; }

    public virtual DbSet<VehicleReceptionRecord> VehicleReceptionRecords { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {

    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Accessory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__accessor__3213E83F52298628");

            entity.ToTable("accessory");

            entity.HasIndex(e => e.Code, "UQ__accessor__357D4CF9256CFFF6").IsUnique();

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
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("image_url");
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

        modelBuilder.Entity<AccessoryGoodsReceipt>(entity =>
        {
            entity.ToTable("accessory_goods_receipt");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.PurchaseRequestId).HasColumnName("purchase_request_id");
            entity.Property(e => e.ReceiptDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("receipt_date");
            entity.Property(e => e.ReceivedBy).HasColumnName("received_by");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValue("Completed")
                .HasColumnName("status");

            entity.HasOne(d => d.Branch).WithMany(p => p.AccessoryGoodsReceipts)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_accessory_goods_receipt_branch");

            entity.HasOne(d => d.PurchaseRequest).WithMany(p => p.AccessoryGoodsReceipts)
                .HasForeignKey(d => d.PurchaseRequestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_accessory_goods_receipt_request");

            entity.HasOne(d => d.ReceivedByNavigation).WithMany(p => p.AccessoryGoodsReceipts)
                .HasForeignKey(d => d.ReceivedBy)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_accessory_goods_receipt_received_by");
        });

        modelBuilder.Entity<AccessoryGoodsReceiptDetail>(entity =>
        {
            entity.ToTable("accessory_goods_receipt_detail");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccessoryId).HasColumnName("accessory_id");
            entity.Property(e => e.ActualUnitPrice)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("actual_unit_price");
            entity.Property(e => e.ReceiptId).HasColumnName("receipt_id");
            entity.Property(e => e.ReceivedQuantity).HasColumnName("received_quantity");
            entity.Property(e => e.StockCondition)
                .HasMaxLength(20)
                .HasDefaultValue("NEW")
                .HasColumnName("stock_condition");

            entity.HasOne(d => d.Accessory).WithMany(p => p.AccessoryGoodsReceiptDetails)
                .HasForeignKey(d => d.AccessoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_accessory_goods_receipt_detail_accessory");

            entity.HasOne(d => d.Receipt).WithMany(p => p.AccessoryGoodsReceiptDetails)
                .HasForeignKey(d => d.ReceiptId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_accessory_goods_receipt_detail_receipt");
        });

        modelBuilder.Entity<AccessoryPurchaseRequest>(entity =>
        {
            entity.ToTable("accessory_purchase_request");

            entity.HasIndex(e => e.RequestCode, "UQ_accessory_purchase_request_request_code").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ApprovedById).HasColumnName("approved_by_id");
            entity.Property(e => e.ApprovedDate)
                .HasColumnType("datetime")
                .HasColumnName("approved_date");
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.RequestCode)
                .HasMaxLength(50)
                .HasColumnName("request_code");
            entity.Property(e => e.RequestDate)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("request_date");
            entity.Property(e => e.RequesterId).HasColumnName("requester_id");
            entity.Property(e => e.Status)
                .HasMaxLength(30)
                .HasDefaultValue("Pending")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.ApprovedBy).WithMany(p => p.AccessoryPurchaseRequestApprovedBies)
                .HasForeignKey(d => d.ApprovedById)
                .HasConstraintName("FK_accessory_purchase_request_approved_by");

            entity.HasOne(d => d.Branch).WithMany(p => p.AccessoryPurchaseRequests)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_accessory_purchase_request_branch");

            entity.HasOne(d => d.Requester).WithMany(p => p.AccessoryPurchaseRequestRequesters)
                .HasForeignKey(d => d.RequesterId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_accessory_purchase_request_requester");
        });

        modelBuilder.Entity<AccessoryPurchaseRequestDetail>(entity =>
        {
            entity.ToTable("accessory_purchase_request_detail");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccessoryId).HasColumnName("accessory_id");
            entity.Property(e => e.ApprovedQuantity).HasColumnName("approved_quantity");
            entity.Property(e => e.EstimatedUnitPrice)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("estimated_unit_price");
            entity.Property(e => e.Notes)
                .HasMaxLength(500)
                .HasColumnName("notes");
            entity.Property(e => e.RequestId).HasColumnName("request_id");
            entity.Property(e => e.RequestedQuantity).HasColumnName("requested_quantity");

            entity.HasOne(d => d.Accessory).WithMany(p => p.AccessoryPurchaseRequestDetails)
                .HasForeignKey(d => d.AccessoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_accessory_purchase_request_detail_accessory");

            entity.HasOne(d => d.Request).WithMany(p => p.AccessoryPurchaseRequestDetails)
                .HasForeignKey(d => d.RequestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_accessory_purchase_request_detail_request");
        });

        modelBuilder.Entity<AccessoryTransaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__accessor__3213E83F77410E60");

            entity.ToTable("accessory_transaction");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccessoryId).HasColumnName("accessory_id");
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.PerformedBy).HasColumnName("performed_by");
            entity.Property(e => e.Quantity).HasColumnName("quantity");
            entity.Property(e => e.ReferenceId).HasColumnName("reference_id");
            entity.Property(e => e.ReferenceType)
                .HasMaxLength(30)
                .HasColumnName("reference_type");
            entity.Property(e => e.StockCondition)
                .HasMaxLength(20)
                .HasColumnName("stock_condition");
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

            entity.HasOne(d => d.Branch).WithMany(p => p.AccessoryTransactions)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("FK_accessory_transaction_branch");

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
            entity.HasKey(e => e.Id).HasName("PK__asset_ch__3213E83FB2591C8E");

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
                .HasConstraintName("FK__asset_cha__accou__07C12930");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.AssetChangeLogs)
                .HasForeignKey(d => d.VehicleId)
                .HasConstraintName("FK__asset_cha__vehic__08B54D69");
        });

        modelBuilder.Entity<Branch>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__branch__3213E83FDDC584C0");

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

        modelBuilder.Entity<BranchAccessoryStock>(entity =>
        {
            entity.ToTable("branch_accessory_stock");

            entity.HasIndex(e => new { e.BranchId, e.AccessoryId, e.StockCondition }, "UQ_branch_accessory_stock_branch_accessory_condition").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccessoryId).HasColumnName("accessory_id");
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.MinimumStock).HasColumnName("minimum_stock");
            entity.Property(e => e.QuantityInStock).HasColumnName("quantity_in_stock");
            entity.Property(e => e.StockCondition)
                .HasMaxLength(20)
                .HasDefaultValue("NEW")
                .HasColumnName("stock_condition");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.Accessory).WithMany(p => p.BranchAccessoryStocks)
                .HasForeignKey(d => d.AccessoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_branch_accessory_stock_accessory");

            entity.HasOne(d => d.Branch).WithMany(p => p.BranchAccessoryStocks)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_branch_accessory_stock_branch");
        });

        modelBuilder.Entity<BulkPurchaseDetail>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__bulk_pur__3213E83FCD59A222");

            entity.ToTable("bulk_purchase_detail");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity.Property(e => e.BranchNotes).HasColumnName("branch_notes");
            entity.Property(e => e.Manufacturer)
                .HasMaxLength(100)
                .HasColumnName("manufacturer");
            entity.Property(e => e.ProposedQuantity).HasColumnName("proposed_quantity");
            entity.Property(e => e.PurchaseProposalId).HasColumnName("purchase_proposal_id");
            entity.Property(e => e.ReceivedDate)
                .HasColumnType("datetime")
                .HasColumnName("received_date");
            entity.Property(e => e.Seats).HasColumnName("seats");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending")
                .HasColumnName("status");
            entity.Property(e => e.UnitPrice)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("unit_price");
            entity.Property(e => e.AcquisitionMethod).HasMaxLength(50).HasColumnName("acquisition_method");
            entity.Property(e => e.Version).HasMaxLength(100).HasColumnName("version");
            entity.Property(e => e.RegistrationTax).HasColumnType("decimal(15, 2)").HasColumnName("registration_tax");
            entity.Property(e => e.RoadMaintenanceFee).HasColumnType("decimal(15, 2)").HasColumnName("road_maintenance_fee");
            entity.Property(e => e.LicensePlateFee).HasColumnType("decimal(15, 2)").HasColumnName("license_plate_fee");
            entity.Property(e => e.InsuranceFee).HasColumnType("decimal(15, 2)").HasColumnName("insurance_fee");
            entity.Property(e => e.HasCamera158).HasDefaultValue(false).HasColumnName("has_camera_158");
            entity.Property(e => e.HasGsht).HasDefaultValue(false).HasColumnName("has_gsht");
            entity.Property(e => e.FuelNorm).HasColumnType("decimal(15, 2)").HasColumnName("fuel_norm");

            entity.HasOne(d => d.Branch).WithMany(p => p.BulkPurchaseDetails)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("FK__bulk_purc__branc__09A971A2");

            entity.HasOne(d => d.PurchaseProposal).WithMany(p => p.BulkPurchaseDetails)
                .HasForeignKey(d => d.PurchaseProposalId)
                .HasConstraintName("FK__bulk_purc__purch__0A9D95DB");
        });

        modelBuilder.Entity<CheckRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__check_re__3213E83FEB2063A4");

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
                .HasConstraintName("FK__check_rec__opera__0B91BA14");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.CheckRecords)
                .HasForeignKey(d => d.VehicleId)
                .HasConstraintName("FK__check_rec__vehic__0C85DE4D");
        });

        modelBuilder.Entity<DepreciationLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__deprecia__3213E83F4577D845");

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
                .HasConstraintName("FK__depreciat__accou__0D7A0286");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.DepreciationLogs)
                .HasForeignKey(d => d.VehicleId)
                .HasConstraintName("FK__depreciat__vehic__0E6E26BF");
        });

        modelBuilder.Entity<DisposalProposal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__disposal__3213E83F8FDA24C1");

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
                .HasConstraintName("FK__disposal___manag__0F624AF8");

            entity.HasOne(d => d.Proposer).WithMany(p => p.DisposalProposalProposers)
                .HasForeignKey(d => d.ProposerId)
                .HasConstraintName("FK__disposal___propo__10566F31");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.DisposalProposals)
                .HasForeignKey(d => d.VehicleId)
                .HasConstraintName("FK__disposal___vehic__114A936A");
        });

        modelBuilder.Entity<Driver>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__driver__3213E83F51C8A93D");

            entity.ToTable("driver");

            entity.HasIndex(e => e.LicenseNumber, "UQ__driver__D482A00336E157FF").IsUnique();

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
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");

            entity.HasOne(d => d.Branch).WithMany(p => p.Drivers)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("FK__driver__branch_i__123EB7A3");
        });

        modelBuilder.Entity<DriverTransferDetail>(entity =>
        {
            entity.ToTable("driver_transfer_detail");

            entity.HasIndex(e => e.DriverId, "IX_driver_transfer_detail_driver");

            entity.HasIndex(e => e.TransferRequestId, "IX_driver_transfer_detail_request");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ConfirmedByUserId).HasColumnName("confirmed_by_user_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DriverId).HasColumnName("driver_id");
            entity.Property(e => e.FromBranchId).HasColumnName("from_branch_id");
            entity.Property(e => e.TransferDate)
                .HasColumnType("datetime")
                .HasColumnName("transfer_date");
            entity.Property(e => e.TransferRequestId).HasColumnName("transfer_request_id");

            entity.HasOne(d => d.ConfirmedByUser).WithMany(p => p.DriverTransferDetailConfirmedByUsers)
                .HasForeignKey(d => d.ConfirmedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_driver_transfer_detail_user");

            entity.HasOne(d => d.Driver).WithMany(p => p.DriverTransferDetails)
                .HasForeignKey(d => d.DriverId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_driver_transfer_detail_driver");

            entity.HasOne(d => d.FromBranch).WithMany(p => p.DriverTransferDetails)
                .HasForeignKey(d => d.FromBranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_driver_transfer_detail_branch");

            entity.HasOne(d => d.TransferRequest).WithMany(p => p.TransferDetails)
                .HasForeignKey(d => d.TransferRequestId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_driver_transfer_detail_request");
        });

        modelBuilder.Entity<DriverTransferRequest>(entity =>
        {
            entity.ToTable("driver_transfer_request");

            entity.HasIndex(e => new { e.RequestingBranchId, e.Status }, "IX_driver_transfer_request_branch_status");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedByUserId).HasColumnName("created_by_user_id");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.FulfilledQuantity)
                .HasDefaultValue(0)
                .HasColumnName("fulfilled_quantity");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.RequestedQuantity).HasColumnName("requested_quantity");
            entity.Property(e => e.RequestingBranchId).HasColumnName("requesting_branch_id");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasDefaultValue("Pending")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.CreatedByUser).WithMany(p => p.DriverTransferRequestCreatedByUsers)
                .HasForeignKey(d => d.CreatedByUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_driver_transfer_request_user");

            entity.HasOne(d => d.RequestingBranch).WithMany(p => p.DriverTransferRequests)
                .HasForeignKey(d => d.RequestingBranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_driver_transfer_request_branch");
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
            entity.HasKey(e => e.Id).HasName("PK__insuranc__3213E83FAF7AEE68");

            entity.ToTable("insurance_record");

            entity.HasIndex(e => e.PolicyNumber, "UQ__insuranc__96916872CD3D007E").IsUnique();

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
                .HasConstraintName("FK__insurance__vehic__14270015");
        });

        modelBuilder.Entity<MaintenanceRequest>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__maintena__3213E83F4B05A4A3");

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
            entity.Property(e => e.CompletionNote)
                .HasMaxLength(1000)
                .HasColumnName("completion_note");
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
                .HasConstraintName("FK_maintenance_request_accountant");

            entity.HasOne(d => d.Operator).WithMany(p => p.MaintenanceRequestOperators)
                .HasForeignKey(d => d.OperatorId)
                .HasConstraintName("FK__maintenan__opera__151B244E");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.MaintenanceRequests)
                .HasForeignKey(d => d.VehicleId)
                .HasConstraintName("FK__maintenan__vehic__160F4887");
        });

        modelBuilder.Entity<OverBudgetRepairProposal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__over_bud__3213E83F7F91129F");

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
                .HasConstraintName("FK__over_budg__maint__17F790F9");

            entity.HasOne(d => d.Manager).WithMany(p => p.OverBudgetRepairProposals)
                .HasForeignKey(d => d.ManagerId)
                .HasConstraintName("FK__over_budg__manag__18EBB532");
        });

        modelBuilder.Entity<PurchaseProposal>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__purchase__3213E83F15934B4C");

            entity.ToTable("purchase_proposal");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ApprovedDate).HasColumnName("approved_date");
            entity.Property(e => e.ChiefAccountantId).HasColumnName("chief_accountant_id");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.CreatedDate).HasColumnName("created_date");
            entity.Property(e => e.CompletionDeadline).HasColumnType("datetime").HasColumnName("completion_deadline");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.ManagerId).HasColumnName("manager_id");
            entity.Property(e => e.ProposedCost)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("proposed_cost");
            entity.Property(e => e.ActualCost)
                .HasColumnType("decimal(15, 2)")
                .HasColumnName("actual_cost");
            entity.Property(e => e.ProposerId).HasColumnName("proposer_id");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");

            entity.HasOne(d => d.ChiefAccountant).WithMany(p => p.PurchaseProposalChiefAccountants)
                .HasForeignKey(d => d.ChiefAccountantId)
                .HasConstraintName("FK__purchase___chief__19DFD96B");

            entity.HasOne(d => d.Manager).WithMany(p => p.PurchaseProposalManagers)
                .HasForeignKey(d => d.ManagerId)
                .HasConstraintName("FK__purchase___manag__1AD3FDA4");

            entity.HasOne(d => d.Proposer).WithMany(p => p.PurchaseProposalProposers)
                .HasForeignKey(d => d.ProposerId)
                .HasConstraintName("FK__purchase___propo__1BC821DD");
        });

        modelBuilder.Entity<RegistrationRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__registra__3213E83FC0FE053C");

            entity.ToTable("registration_record");

            entity.HasIndex(e => e.RegistrationNumber, "UQ__registra__125DB2A34DED6B13").IsUnique();

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
                .HasConstraintName("FK__registrat__vehic__1CBC4616");
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__role__3213E83FA3027031");

            entity.ToTable("role");

            entity.HasIndex(e => e.Name, "UQ__role__72E12F1BCF9A3881").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.Description).HasColumnName("description");
            entity.Property(e => e.Name)
                .HasMaxLength(50)
                .HasColumnName("name");
        });

        modelBuilder.Entity<TransferPlan>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__transfer__3213E83FED31735B");

            entity.ToTable("transfer_plan");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.CheckinByUserId).HasColumnName("checkin_by_user_id");
            entity.Property(e => e.CheckinDate)
                .HasColumnType("datetime")
                .HasColumnName("checkin_date");
            entity.Property(e => e.CheckinNote)
                .HasMaxLength(500)
                .HasColumnName("checkin_note");
            entity.Property(e => e.CheckoutByUserId).HasColumnName("checkout_by_user_id");
            entity.Property(e => e.CheckoutDate)
                .HasColumnType("datetime")
                .HasColumnName("checkout_date");
            entity.Property(e => e.CheckoutNote)
                .HasMaxLength(500)
                .HasColumnName("checkout_note");
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
            entity.Property(e => e.PlannedArrivalDate)
                .HasColumnType("datetime")
                .HasColumnName("planned_arrival_date");
            entity.Property(e => e.PlannedDepartureDate)
                .HasColumnType("datetime")
                .HasColumnName("planned_departure_date");
            entity.Property(e => e.Status)
                .HasMaxLength(20)
                .HasColumnName("status");
            entity.Property(e => e.ToBranchId).HasColumnName("to_branch_id");
            entity.Property(e => e.UpdatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");

            entity.HasOne(d => d.CheckinByUser).WithMany(p => p.TransferPlanCheckinByUsers)
                .HasForeignKey(d => d.CheckinByUserId)
                .HasConstraintName("FK_transfer_plan_checkin_by");

            entity.HasOne(d => d.CheckoutByUser).WithMany(p => p.TransferPlanCheckoutByUsers)
                .HasForeignKey(d => d.CheckoutByUserId)
                .HasConstraintName("FK_transfer_plan_checkout_by");

            entity.HasOne(d => d.FromBranch).WithMany(p => p.TransferPlanFromBranches)
                .HasForeignKey(d => d.FromBranchId)
                .HasConstraintName("FK__transfer___from___1DB06A4F");

            entity.HasOne(d => d.Manager).WithMany(p => p.TransferPlanManagers)
                .HasForeignKey(d => d.ManagerId)
                .HasConstraintName("FK__transfer___manag__1EA48E88");

            entity.HasOne(d => d.ToBranch).WithMany(p => p.TransferPlanToBranches)
                .HasForeignKey(d => d.ToBranchId)
                .HasConstraintName("FK__transfer___to_br__1F98B2C1");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.TransferPlans)
                .HasForeignKey(d => d.VehicleId)
                .HasConstraintName("FK__transfer___vehic__208CD6FA");
        });

        modelBuilder.Entity<TripLog>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__trip_log__3213E83F1B150846");

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
            entity.Property(e => e.EndedBy).HasColumnName("ended_by");
            entity.Property(e => e.Origin)
                .HasMaxLength(255)
                .HasColumnName("origin");
            entity.Property(e => e.StartMileage)
                .HasColumnType("decimal(10, 2)")
                .HasColumnName("start_mileage");
            entity.Property(e => e.StartTime)
                .HasColumnType("datetime")
                .HasColumnName("start_time");
            entity.Property(e => e.StartedBy).HasColumnName("started_by");
            entity.Property(e => e.TransferPlanId).HasColumnName("transfer_plan_id");
            entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");

            entity.HasOne(d => d.Driver).WithMany(p => p.TripLogs)
                .HasForeignKey(d => d.DriverId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Trip_Driver");

            entity.HasOne(d => d.TransferPlan).WithMany(p => p.TripLogs)
                .HasForeignKey(d => d.TransferPlanId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_trip_log_transfer_plan");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.TripLogs)
                .HasForeignKey(d => d.VehicleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Trip_Vehicle");
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__user__3213E83F767DBADB");

            entity.ToTable("user");

            entity.HasIndex(e => e.Email, "UQ__user__AB6E61647784697C").IsUnique();

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
                .HasConstraintName("FK__user__branch_id__2180FB33");

            entity.HasMany(d => d.Roles).WithMany(p => p.Users)
                .UsingEntity<Dictionary<string, object>>(
                    "UserRole",
                    r => r.HasOne<Role>().WithMany()
                        .HasForeignKey("RoleId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__user_role__role___22751F6C"),
                    l => l.HasOne<User>().WithMany()
                        .HasForeignKey("UserId")
                        .OnDelete(DeleteBehavior.ClientSetNull)
                        .HasConstraintName("FK__user_role__user___236943A5"),
                    j =>
                    {
                        j.HasKey("UserId", "RoleId").HasName("PK__user_rol__6EDEA153F1CBD4AC");
                        j.ToTable("user_role");
                        j.IndexerProperty<int>("UserId").HasColumnName("user_id");
                        j.IndexerProperty<int>("RoleId").HasColumnName("role_id");
                    });
        });

        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__vehicle__3213E83FCC21A0A7");

            entity.ToTable("vehicle");

            entity.HasIndex(e => e.LicensePlate, "UQ__vehicle__F72CD56EBDF8FA64").IsUnique();

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
            entity.Property(e => e.ImageUrl)
                .HasMaxLength(500)
                .HasColumnName("image_url");
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
            entity.Property(e => e.Vin).HasMaxLength(100).HasColumnName("vin");
            entity.Property(e => e.ChassisNumber).HasMaxLength(100).HasColumnName("chassis_number");
            entity.Property(e => e.EngineNumber).HasMaxLength(100).HasColumnName("engine_number");
            entity.Property(e => e.TelematicsImei).HasMaxLength(100).HasColumnName("telematics_imei");
            entity.Property(e => e.RegistrationExpirationDate).HasColumnType("date").HasColumnName("registration_expiration_date");
            entity.Property(e => e.InsuranceExpirationDate).HasColumnType("date").HasColumnName("insurance_expiration_date");
            entity.Property(e => e.BadgeType).HasMaxLength(50).HasColumnName("badge_type");
            entity.Property(e => e.BadgeExpirationDate).HasColumnType("date").HasColumnName("badge_expiration_date");
            entity.Property(e => e.FuelNorm).HasColumnType("decimal(10, 2)").HasColumnName("fuel_norm");

            entity.HasOne(d => d.CurrentBranch).WithMany(p => p.Vehicles)
                .HasForeignKey(d => d.CurrentBranchId)
                .HasConstraintName("FK__vehicle__current__245D67DE");

            entity.HasOne(d => d.CurrentDriver).WithMany(p => p.Vehicles)
                .HasForeignKey(d => d.CurrentDriverId)
                .HasConstraintName("FK__vehicle__current__25518C17");

            entity.HasOne(d => d.Model).WithMany(p => p.Vehicles)
                .HasForeignKey(d => d.ModelId)
                .HasConstraintName("FK__vehicle__model_i__2645B050");
        });

        modelBuilder.Entity<VehicleAccessory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__vehicle___3213E83F1126D7CF");

            entity.ToTable("vehicle_accessory");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccessoryId).HasColumnName("accessory_id");
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
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
            entity.Property(e => e.SourceTransactionId).HasColumnName("source_transaction_id");
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
                .HasConstraintName("FK__vehicle_a__acces__2739D489");

            entity.HasOne(d => d.Branch).WithMany(p => p.VehicleAccessories)
                .HasForeignKey(d => d.BranchId)
                .HasConstraintName("FK_vehicle_accessory_branch");

            entity.HasOne(d => d.InstalledByNavigation).WithMany(p => p.VehicleAccessoryInstalledByNavigations)
                .HasForeignKey(d => d.InstalledBy)
                .HasConstraintName("FK_vehicle_accessory_installed_by");

            entity.HasOne(d => d.RemovedByNavigation).WithMany(p => p.VehicleAccessoryRemovedByNavigations)
                .HasForeignKey(d => d.RemovedBy)
                .HasConstraintName("FK_vehicle_accessory_removed_by");

            entity.HasOne(d => d.SourceTransaction).WithMany(p => p.VehicleAccessories)
                .HasForeignKey(d => d.SourceTransactionId)
                .HasConstraintName("FK_vehicle_accessory_source_transaction");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.VehicleAccessories)
                .HasForeignKey(d => d.VehicleId)
                .HasConstraintName("FK__vehicle_a__vehic__282DF8C2");
        });

        modelBuilder.Entity<VehicleAccessoryRequirement>(entity =>
        {
            entity.ToTable("vehicle_accessory_requirement");

            entity.HasIndex(e => new { e.ModelId, e.AccessoryId }, "UQ_vehicle_accessory_requirement_model_accessory").IsUnique();

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AccessoryId).HasColumnName("accessory_id");
            entity.Property(e => e.IsMandatory)
                .HasDefaultValue(true)
                .HasColumnName("is_mandatory");
            entity.Property(e => e.ModelId).HasColumnName("model_id");
            entity.Property(e => e.Notes)
                .HasMaxLength(500)
                .HasColumnName("notes");
            entity.Property(e => e.RequiredQuantity).HasColumnName("required_quantity");

            entity.HasOne(d => d.Accessory).WithMany(p => p.VehicleAccessoryRequirements)
                .HasForeignKey(d => d.AccessoryId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_vehicle_accessory_requirement_accessory");

            entity.HasOne(d => d.Model).WithMany(p => p.VehicleAccessoryRequirements)
                .HasForeignKey(d => d.ModelId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_vehicle_accessory_requirement_model");
        });

        modelBuilder.Entity<VehicleDriverHistory>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__vehicle___3213E83FD10F0234");

            entity.ToTable("vehicle_driver_history");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.AssignDate).HasColumnName("assign_date");
            entity.Property(e => e.DriverId).HasColumnName("driver_id");
            entity.Property(e => e.Notes).HasColumnName("notes");
            entity.Property(e => e.UnassignDate).HasColumnName("unassign_date");
            entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");

            entity.HasOne(d => d.Driver).WithMany(p => p.VehicleDriverHistories)
                .HasForeignKey(d => d.DriverId)
                .HasConstraintName("FK__vehicle_d__drive__29221CFB");

            entity.HasOne(d => d.Vehicle).WithMany(p => p.VehicleDriverHistories)
                .HasForeignKey(d => d.VehicleId)
                .HasConstraintName("FK__vehicle_d__vehic__2A164134");
        });

        modelBuilder.Entity<VehicleModel>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__vehicle___3213E83FA8318D31");

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
            entity.Property(e => e.EnginePower).HasMaxLength(100).HasColumnName("engine_power");
            entity.Property(e => e.EmissionStandard).HasMaxLength(50).HasColumnName("emission_standard");
            entity.Property(e => e.PayloadCapacity).HasColumnType("decimal(18, 2)").HasColumnName("payload_capacity");
            entity.Property(e => e.FuelType).HasMaxLength(50).HasColumnName("fuel_type");
        });

        modelBuilder.Entity<VehicleReceptionRecord>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__vehicle___3213E83FD5D86D81");

            entity.ToTable("vehicle_reception_record");

            entity.HasIndex(e => e.BranchId, "IX_VehicleReception_BranchId");

            entity.HasIndex(e => e.PurchaseProposalId, "IX_VehicleReception_PurchaseProposalId");

            entity.HasIndex(e => e.Status, "IX_VehicleReception_Status");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity.Property(e => e.ChassisNumber)
                .HasMaxLength(100)
                .HasColumnName("chassis_number");
            entity.Property(e => e.CreatedAt)
                .HasDefaultValueSql("(getdate())")
                .HasColumnType("datetime")
                .HasColumnName("created_at");
            entity.Property(e => e.DeletedAt)
                .HasColumnType("datetime")
                .HasColumnName("deleted_at");
            entity.Property(e => e.EngineNumber)
                .HasMaxLength(100)
                .HasColumnName("engine_number");
            entity.Property(e => e.LicensePlate)
                .HasMaxLength(50)
                .HasColumnName("license_plate");
            entity.Property(e => e.Notes)
                .HasMaxLength(1000)
                .HasColumnName("notes");
            entity.Property(e => e.OperatorId).HasColumnName("operator_id");
            entity.Property(e => e.PurchaseProposalId).HasColumnName("purchase_proposal_id");
            entity.Property(e => e.Reason).HasColumnName("reason");
            entity.Property(e => e.ReceiptImageUrl).HasColumnName("receipt_image_url");
            entity.Property(e => e.ReceivedDate).HasColumnName("received_date");
            entity.Property(e => e.RequestedDate).HasColumnName("requested_date");
            entity.Property(e => e.Status)
                .HasMaxLength(50)
                .HasDefaultValue("Pending")
                .HasColumnName("status");
            entity.Property(e => e.UpdatedAt)
                .HasColumnType("datetime")
                .HasColumnName("updated_at");
            entity.Property(e => e.Version).HasMaxLength(100).HasColumnName("version");
            entity.Property(e => e.Vin).HasMaxLength(100).HasColumnName("vin");
            entity.Property(e => e.TelematicsImei).HasMaxLength(100).HasColumnName("telematics_imei");
            entity.Property(e => e.RegistrationExpirationDate).HasColumnType("date").HasColumnName("registration_expiration_date");
            entity.Property(e => e.InsuranceExpirationDate).HasColumnType("date").HasColumnName("insurance_expiration_date");
            entity.Property(e => e.BadgeType).HasMaxLength(50).HasColumnName("badge_type");
            entity.Property(e => e.BadgeExpirationDate).HasColumnType("date").HasColumnName("badge_expiration_date");
            entity.Property(e => e.FuelNorm).HasColumnType("decimal(10, 2)").HasColumnName("fuel_norm");
            entity.Property(e => e.YearManufacture).HasColumnName("year_manufacture");
            entity.Property(e => e.Mileage).HasColumnType("decimal(18, 2)").HasColumnName("mileage");

            entity.HasOne(d => d.Branch).WithMany(p => p.VehicleReceptionRecords)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__vehicle_reception__branch");

            entity.HasOne(d => d.Operator).WithMany(p => p.VehicleReceptionRecords)
                .HasForeignKey(d => d.OperatorId)
                .HasConstraintName("FK__vehicle_reception__operator");

            entity.HasOne(d => d.PurchaseProposal).WithMany(p => p.VehicleReceptionRecords)
                .HasForeignKey(d => d.PurchaseProposalId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__vehicle_reception__proposal");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
