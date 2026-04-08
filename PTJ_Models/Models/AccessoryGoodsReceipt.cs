using System;
using System.Collections.Generic;

namespace Models.Models;

public partial class AccessoryGoodsReceipt
{
    public int Id { get; set; }

    public int PurchaseRequestId { get; set; }

    public int BranchId { get; set; }

    public int ReceivedBy { get; set; }

    public DateTime ReceiptDate { get; set; }

    public string Status { get; set; } = null!;

    public string? Notes { get; set; }

    public DateTime CreatedAt { get; set; }

    public virtual ICollection<AccessoryGoodsReceiptDetail> AccessoryGoodsReceiptDetails { get; set; } = new List<AccessoryGoodsReceiptDetail>();

    public virtual Branch Branch { get; set; } = null!;

    public virtual AccessoryPurchaseRequest PurchaseRequest { get; set; } = null!;

    public virtual User ReceivedByNavigation { get; set; } = null!;
}
