using System;
using System.Collections.Generic;

namespace Models.Models;

public partial class Branch
{
    public int Id { get; set; }

    public string? Name { get; set; }

    public string? Address { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual ICollection<BulkPurchaseDetail> BulkPurchaseDetails { get; set; } = new List<BulkPurchaseDetail>();

    public virtual ICollection<Driver> Drivers { get; set; } = new List<Driver>();

    public virtual ICollection<TransferPlan> TransferPlanFromBranches { get; set; } = new List<TransferPlan>();

    public virtual ICollection<TransferPlan> TransferPlanToBranches { get; set; } = new List<TransferPlan>();

    public virtual ICollection<User> Users { get; set; } = new List<User>();

    public virtual ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
}
