using System;
using System.Collections.Generic;

namespace Models.Models;

public partial class AssetChangeLog
{
    public int Id { get; set; }

    public int? VehicleId { get; set; }

    public string? ChangeType { get; set; }

    public DateOnly? ChangeDate { get; set; }

    public decimal? AmountChange { get; set; }

    public string? Reason { get; set; }

    public int? AccountantId { get; set; }

    public virtual User? Accountant { get; set; }

    public virtual Vehicle? Vehicle { get; set; }
}
