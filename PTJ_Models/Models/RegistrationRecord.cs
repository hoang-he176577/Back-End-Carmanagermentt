using System;
using System.Collections.Generic;

namespace Models.Models;

public partial class RegistrationRecord
{
    public int Id { get; set; }

    public int? VehicleId { get; set; }

    public string? RegistrationNumber { get; set; }

    public string? Authority { get; set; }

    public DateOnly? IssueDate { get; set; }

    public DateOnly? ExpiryDate { get; set; }

    public decimal? Cost { get; set; }

    public string? Notes { get; set; }

    public DateTime? CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public DateTime? DeletedAt { get; set; }

    public virtual Vehicle? Vehicle { get; set; }
}
