namespace Models.DTO.PendingRequests;

public sealed class PendingRequestDto
{
    public int Id { get; set; }

    /// <summary>
    /// "Purchase", "Disposal", "Maintenance", "OverBudgetRepair", "Transfer"
    /// </summary>
    public string Type { get; set; } = string.Empty;

    public string? Status { get; set; }

    public string? Description { get; set; }

    /// <summary>
    /// Proposed cost / price / over-budget amount depending on the type.
    /// </summary>
    public decimal? Amount { get; set; }

    public string? ProposerName { get; set; }

    public DateOnly? RequestDate { get; set; }

    public DateTime? CreatedAt { get; set; }

    public string? VehicleLicensePlate { get; set; }
}
