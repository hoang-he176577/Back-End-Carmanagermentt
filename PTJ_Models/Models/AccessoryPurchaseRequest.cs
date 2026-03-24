using System;
using System.Collections.Generic;

namespace Models.Models;

public partial class AccessoryPurchaseRequest
{
    public int Id { get; set; }

    public string RequestCode { get; set; } = null!;

    public int BranchId { get; set; }

    public int RequesterId { get; set; }

    public int? ApprovedById { get; set; }

    public DateTime RequestDate { get; set; }

    public DateTime? ApprovedDate { get; set; }

    public string Status { get; set; } = null!;

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ICollection<AccessoryGoodsReceipt> AccessoryGoodsReceipts { get; set; } = new List<AccessoryGoodsReceipt>();

    public virtual ICollection<AccessoryPurchaseRequestDetail> AccessoryPurchaseRequestDetails { get; set; } = new List<AccessoryPurchaseRequestDetail>();

    public virtual User? ApprovedBy { get; set; }

    public virtual Branch Branch { get; set; } = null!;

    public virtual User Requester { get; set; } = null!;
}
