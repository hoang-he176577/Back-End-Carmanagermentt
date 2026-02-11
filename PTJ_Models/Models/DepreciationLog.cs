using System;
using System.Collections.Generic;

namespace Models.Models;

public partial class DepreciationLog
{
    public int Id { get; set; }

    public int? VehicleId { get; set; }

    public DateOnly? CalculationDate { get; set; }

    public decimal? DepreciationAmount { get; set; }

    public decimal? NewValue { get; set; }

    public int? AccountantId { get; set; }

    public virtual User? Accountant { get; set; }

    public virtual Vehicle? Vehicle { get; set; }
}
