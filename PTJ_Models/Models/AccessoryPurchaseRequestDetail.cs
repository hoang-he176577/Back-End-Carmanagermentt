using System;
using System.Collections.Generic;

namespace Models.Models;

public partial class AccessoryPurchaseRequestDetail
{
    public int Id { get; set; }

    public int RequestId { get; set; }

    public int AccessoryId { get; set; }

    public int RequestedQuantity { get; set; }

    public int? ApprovedQuantity { get; set; }

    public decimal? EstimatedUnitPrice { get; set; }

    public string? Notes { get; set; }

    public virtual Accessory Accessory { get; set; } = null!;

    public virtual AccessoryPurchaseRequest Request { get; set; } = null!;
}
