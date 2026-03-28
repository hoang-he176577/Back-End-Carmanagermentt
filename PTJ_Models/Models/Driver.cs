using System;
using System.Collections.Generic;

namespace Models.Models;

public partial class Driver
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? LicenseNumber { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public DateOnly? HireDate { get; set; }

    public string? Status { get; set; }

    public int? BranchId { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Branch? Branch { get; set; }

    public virtual ICollection<DriverTransferDetail> DriverTransferDetails { get; set; } = new List<DriverTransferDetail>();

    public virtual ICollection<TripLog> TripLogs { get; set; } = new List<TripLog>();

    public virtual ICollection<VehicleDriverHistory> VehicleDriverHistories { get; set; } = new List<VehicleDriverHistory>();

    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
