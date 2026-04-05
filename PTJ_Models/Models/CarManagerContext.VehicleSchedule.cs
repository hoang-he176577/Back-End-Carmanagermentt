using Microsoft.EntityFrameworkCore;

namespace Models.Models;

public partial class CarManagerContext
{
    public virtual DbSet<VehicleSchedule> VehicleSchedules { get; set; } = null!;
    public virtual DbSet<VehicleScheduleAudit> VehicleScheduleAudits { get; set; } = null!;

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<VehicleSchedule>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__vehicle_schedule__id");

            entity.ToTable("vehicle_schedule");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.VehicleId).HasColumnName("vehicle_id");
            entity.Property(e => e.DriverId).HasColumnName("driver_id");
            entity.Property(e => e.BranchId).HasColumnName("branch_id");
            entity.Property(e => e.PlannedStartTime).HasColumnName("planned_start_time");
            entity.Property(e => e.PlannedEndTime).HasColumnName("planned_end_time");
            entity.Property(e => e.ActualStartTime).HasColumnName("actual_start_time");
            entity.Property(e => e.ActualEndTime).HasColumnName("actual_end_time");
            entity.Property(e => e.Origin).HasMaxLength(255).HasColumnName("origin");
            entity.Property(e => e.Destination).HasMaxLength(255).HasColumnName("destination");
            entity.Property(e => e.Status).HasMaxLength(30).HasColumnName("status");
            entity.Property(e => e.ExtensionMinutes).HasColumnName("extension_minutes");
            entity.Property(e => e.ExtensionReason).HasMaxLength(500).HasColumnName("extension_reason");
            entity.Property(e => e.SwapFromScheduleId).HasColumnName("swap_from_schedule_id");
            entity.Property(e => e.SwappedVehicleId).HasColumnName("swapped_vehicle_id");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");
            entity.Property(e => e.UpdatedAt).HasColumnName("updated_at");
            entity.Property(e => e.DeletedAt).HasColumnName("deleted_at");

            entity.HasOne(d => d.Branch)
                .WithMany(p => p.VehicleSchedules)
                .HasForeignKey(d => d.BranchId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_vehicle_schedule_branch");

            entity.HasOne(d => d.Driver)
                .WithMany(p => p.VehicleSchedules)
                .HasForeignKey(d => d.DriverId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_vehicle_schedule_driver");

            entity.HasOne(d => d.Vehicle)
                .WithMany(p => p.VehicleSchedules)
                .HasForeignKey(d => d.VehicleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_vehicle_schedule_vehicle");

            entity.HasOne(d => d.SwappedVehicle)
                .WithMany(p => p.VehicleSchedulesAsSwap)
                .HasForeignKey(d => d.SwappedVehicleId)
                .HasConstraintName("FK_vehicle_schedule_swapped_vehicle");

            entity.HasOne(d => d.SwapFromSchedule)
                .WithMany(p => p.SwapToSchedules)
                .HasForeignKey(d => d.SwapFromScheduleId)
                .HasConstraintName("FK_vehicle_schedule_swap_from");
        });

        modelBuilder.Entity<VehicleScheduleAudit>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__vehicle_schedule_audit__id");

            entity.ToTable("vehicle_schedule_audit");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.ScheduleId).HasColumnName("schedule_id");
            entity.Property(e => e.ActorUserId).HasColumnName("actor_user_id");
            entity.Property(e => e.Action).HasMaxLength(50).HasColumnName("action");
            entity.Property(e => e.Note).HasMaxLength(500).HasColumnName("note");
            entity.Property(e => e.DataJson).HasColumnName("data_json");
            entity.Property(e => e.CreatedAt).HasColumnName("created_at");

            entity.HasOne(d => d.Schedule)
                .WithMany(p => p.VehicleScheduleAudits)
                .HasForeignKey(d => d.ScheduleId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_vehicle_schedule_audit_schedule");

            entity.HasOne(d => d.ActorUser)
                .WithMany(p => p.VehicleScheduleAudits)
                .HasForeignKey(d => d.ActorUserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_vehicle_schedule_audit_user");
        });
    }
}

