using System;
using System.Collections.Generic;

namespace Models.Models;

public partial class BranchAccessoryStock
{
    public int Id { get; set; }

    public int BranchId { get; set; }

    public int AccessoryId { get; set; }

    public int QuantityInStock { get; set; }

    public int? MinimumStock { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string StockCondition { get; set; } = null!;

    public virtual Accessory Accessory { get; set; } = null!;

    public virtual Branch Branch { get; set; } = null!;
}
