using System.ComponentModel.DataAnnotations;

namespace Models.DTO.DisposalProposals;

public sealed class DisposalProposalDto
{
    public int Id { get; set; }
    public int? VehicleId { get; set; }
    public string? VehicleLicensePlate { get; set; }
    public int? ProposerId { get; set; }
    public string? ProposerName { get; set; }
    public int? ManagerId { get; set; }
    public string? ManagerName { get; set; }
    public DateOnly? CreatedDate { get; set; }
    public DateOnly? ApprovedDate { get; set; }
    public string? Status { get; set; }
    public decimal? ProposedPrice { get; set; }
    public string? Reason { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public sealed class DisposalProposalPagedResultDto
{
    public List<DisposalProposalDto> Items { get; set; } = new();
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalCount { get; set; }
}

public sealed class DisposalProposalListQueryDto
{
    public string? Status { get; set; }
    public int? VehicleId { get; set; }
    public int? ProposerId { get; set; }
    public DateOnly? FromDate { get; set; }
    public DateOnly? ToDate { get; set; }

    [Range(1, int.MaxValue)]
    public int? Page { get; set; }

    [Range(1, 200)]
    public int? PageSize { get; set; }
}

public sealed class DisposalProposalCreateRequestDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int VehicleId { get; set; }

    [Range(0, double.MaxValue)]
    public decimal ProposedPrice { get; set; }

    [Required]
    [MinLength(1)]
    public string Reason { get; set; } = string.Empty;
}

public sealed class DisposalProposalApproveRequestDto
{
    [Range(1, int.MaxValue)]
    public int? AccountantId { get; set; }

    public string? Notes { get; set; }

    public DateOnly? ChangeDate { get; set; }
}

public sealed class DisposalProposalRejectRequestDto
{
    public string? RejectNote { get; set; }
}
