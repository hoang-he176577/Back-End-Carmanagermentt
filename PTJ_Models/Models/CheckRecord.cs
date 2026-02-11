using System;
using System.Collections.Generic;

namespace Models.Models;

public partial class CheckRecord
{
    public int Id { get; set; }

    public int? VehicleId { get; set; }

    public int? OperatorId { get; set; }

    public string? Type { get; set; }

    public DateTime? RecordDate { get; set; }

    public decimal? MileageAtRecord { get; set; }

    public string? Notes { get; set; }

    public virtual User? Operator { get; set; }

    public virtual Vehicle? Vehicle { get; set; }
}
