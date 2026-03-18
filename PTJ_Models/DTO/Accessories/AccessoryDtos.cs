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
}

public class AccessoryImportRequestDto
{
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    public string? Notes { get; set; }
    public int? PerformedBy { get; set; }
}
