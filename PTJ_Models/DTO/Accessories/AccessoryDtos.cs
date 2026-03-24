using System.ComponentModel.DataAnnotations;

namespace Models.DTO.Accessories;

public class AccessoryDto
{
    public int Id { get; set; }
    public string? Code { get; set; }
    public string? Name { get; set; }
    public string Type { get; set; } = string.Empty;
    public int QuantityInStock { get; set; }
    public decimal? UnitPrice { get; set; }
    public int? MinimumStock { get; set; }
    public bool IsActive { get; set; }
    public string? ImageUrl { get; set; }
    public DateTime? CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class AccessoryCreateRequestDto
{
    [Required]
    [StringLength(50)]
    public string Code { get; set; } = string.Empty;

    [Required]
    [StringLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string Type { get; set; } = string.Empty;

    [Range(0, int.MaxValue)]
    public int QuantityInStock { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? UnitPrice { get; set; }

    [Range(0, int.MaxValue)]
    public int? MinimumStock { get; set; }

    public bool IsActive { get; set; } = true;

    [StringLength(500)]
    public string? ImageUrl { get; set; }
}

public class AccessoryUpdateRequestDto
{
    [StringLength(50)]
    public string? Code { get; set; }

    [StringLength(200)]
    public string? Name { get; set; }

    [StringLength(20)]
    public string? Type { get; set; }

    [Range(0, int.MaxValue)]
    public int? QuantityInStock { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? UnitPrice { get; set; }

    [Range(0, int.MaxValue)]
    public int? MinimumStock { get; set; }

    public bool? IsActive { get; set; }

    [StringLength(500)]
    public string? ImageUrl { get; set; }
}

public class AccessoryImportRequestDto
{
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    public string? Notes { get; set; }
    public int? PerformedBy { get; set; }
    public int? BranchId { get; set; }
}

public class BranchAccessoryStockDto
{
    public int Id { get; set; }
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public int AccessoryId { get; set; }
    public string? AccessoryCode { get; set; }
    public string? AccessoryName { get; set; }
    public string? AccessoryType { get; set; }
    public string? ImageUrl { get; set; }
    public int QuantityInStock { get; set; }
    public int? MinimumStock { get; set; }
    public bool IsBelowMinimum { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}

public class BranchAccessoryStockUpsertRequestDto
{
    [Range(1, int.MaxValue)]
    public int BranchId { get; set; }

    [Range(1, int.MaxValue)]
    public int AccessoryId { get; set; }

    [Range(0, int.MaxValue)]
    public int QuantityInStock { get; set; }

    [Range(0, int.MaxValue)]
    public int? MinimumStock { get; set; }
}

public class AccessoryPurchaseRequestDetailRequestDto
{
    [Range(1, int.MaxValue)]
    public int AccessoryId { get; set; }

    [Range(1, int.MaxValue)]
    public int RequestedQuantity { get; set; }

    [Range(0, int.MaxValue)]
    public int? ApprovedQuantity { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? EstimatedUnitPrice { get; set; }

    [StringLength(500)]
    public string? Notes { get; set; }
}

public class AccessoryPurchaseRequestDetailDto
{
    public int Id { get; set; }
    public int AccessoryId { get; set; }
    public string? AccessoryCode { get; set; }
    public string? AccessoryName { get; set; }
    public string? ImageUrl { get; set; }
    public int RequestedQuantity { get; set; }
    public int? ApprovedQuantity { get; set; }
    public int ReceivedQuantity { get; set; }
    public decimal? EstimatedUnitPrice { get; set; }
    public string? Notes { get; set; }
}

public class AccessoryPurchaseRequestCreateRequestDto
{
    public int? BranchId { get; set; }
    public string? Notes { get; set; }

    [MinLength(1)]
    public List<AccessoryPurchaseRequestDetailRequestDto> Details { get; set; } = new();
}

public class AccessoryPurchaseRequestUpdateRequestDto
{
    public string? Notes { get; set; }

    [MinLength(1)]
    public List<AccessoryPurchaseRequestDetailRequestDto> Details { get; set; } = new();
}

public class AccessoryPurchaseRequestApprovalDetailDto
{
    [Range(1, int.MaxValue)]
    public int AccessoryId { get; set; }

    [Range(0, int.MaxValue)]
    public int ApprovedQuantity { get; set; }
}

public class AccessoryPurchaseRequestApproveRequestDto
{
    public string? Notes { get; set; }

    [MinLength(1)]
    public List<AccessoryPurchaseRequestApprovalDetailDto> Details { get; set; } = new();
}

public class AccessoryPurchaseRequestRejectRequestDto
{
    public string? Notes { get; set; }
}

public class AccessoryPurchaseRequestDto
{
    public int Id { get; set; }
    public string RequestCode { get; set; } = string.Empty;
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public int RequesterId { get; set; }
    public string? RequesterName { get; set; }
    public int? ApprovedById { get; set; }
    public string? ApprovedByName { get; set; }
    public DateTime RequestDate { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<AccessoryPurchaseRequestDetailDto> Details { get; set; } = new();
}

public class AccessoryGoodsReceiptDetailRequestDto
{
    [Range(1, int.MaxValue)]
    public int AccessoryId { get; set; }

    [Range(1, int.MaxValue)]
    public int ReceivedQuantity { get; set; }

    [Range(0, double.MaxValue)]
    public decimal? ActualUnitPrice { get; set; }
}

public class AccessoryGoodsReceiptCreateRequestDto
{
    [Range(1, int.MaxValue)]
    public int PurchaseRequestId { get; set; }

    public int? BranchId { get; set; }
    public string? Notes { get; set; }

    [MinLength(1)]
    public List<AccessoryGoodsReceiptDetailRequestDto> Details { get; set; } = new();
}

public class AccessoryGoodsReceiptCompleteRequestDto
{
    [MinLength(1)]
    public List<AccessoryGoodsReceiptDetailRequestDto> Details { get; set; } = new();

    public string? Notes { get; set; }
}

public class AccessoryGoodsReceiptDetailDto
{
    public int Id { get; set; }
    public int AccessoryId { get; set; }
    public string? AccessoryCode { get; set; }
    public string? AccessoryName { get; set; }
    public string? ImageUrl { get; set; }
    public int ReceivedQuantity { get; set; }
    public decimal? ActualUnitPrice { get; set; }
}

public class AccessoryGoodsReceiptDto
{
    public int Id { get; set; }
    public int PurchaseRequestId { get; set; }
    public string? PurchaseRequestCode { get; set; }
    public string? PurchaseRequestStatus { get; set; }
    public int BranchId { get; set; }
    public string? BranchName { get; set; }
    public int ReceivedBy { get; set; }
    public string? ReceivedByName { get; set; }
    public DateTime ReceiptDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
    public List<AccessoryGoodsReceiptDetailDto> Details { get; set; } = new();
}

public class VehicleAccessoryRequirementDto
{
    public int Id { get; set; }
    public int ModelId { get; set; }
    public string? ModelName { get; set; }
    public int AccessoryId { get; set; }
    public string? AccessoryCode { get; set; }
    public string? AccessoryName { get; set; }
    public string? ImageUrl { get; set; }
    public int RequiredQuantity { get; set; }
    public bool IsMandatory { get; set; }
    public string? Notes { get; set; }
}

public class VehicleAccessoryRequirementUpsertRequestDto
{
    [Range(1, int.MaxValue)]
    public int ModelId { get; set; }

    [Range(1, int.MaxValue)]
    public int AccessoryId { get; set; }

    [Range(1, int.MaxValue)]
    public int RequiredQuantity { get; set; }

    public bool IsMandatory { get; set; }
    public string? Notes { get; set; }
}

public class VehicleAccessoryRequirementCheckItemDto
{
    public int AccessoryId { get; set; }
    public string? AccessoryCode { get; set; }
    public string? AccessoryName { get; set; }
    public string? ImageUrl { get; set; }
    public int RequiredQuantity { get; set; }
    public int InstalledQuantity { get; set; }
    public int MissingQuantity { get; set; }
    public bool IsMandatory { get; set; }
    public bool IsSatisfied { get; set; }
    public string? Notes { get; set; }
}

public class VehicleAccessoryRequirementCheckResultDto
{
    public int VehicleId { get; set; }
    public string? VehicleLicensePlate { get; set; }
    public int? ModelId { get; set; }
    public string? ModelName { get; set; }
    public List<VehicleAccessoryRequirementCheckItemDto> Items { get; set; } = new();
}
