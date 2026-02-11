using System;
using System.Collections.Generic;

namespace Models.Models;

public partial class InsuranceRecord
{
    public int Id { get; set; }

    public int? VehicleId { get; set; }

    public string? PolicyNumber { get; set; }

    public string? Provider { get; set; }

    public DateOnly? StartDate { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public decimal? Cost { get; set; }

    public string? CoverageDetails { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Vehicle? Vehicle { get; set; }
}
